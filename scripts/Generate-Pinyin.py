"""Regenerate the offline search table from pinned Unicode Unihan data (Python 3).

Not required to build or run the mod. No game data or item instances are used.
"""
import hashlib
import io
from pathlib import Path
import unicodedata
import urllib.request
import zipfile

VERSION = "17.0.0"
SOURCE = f"https://www.unicode.org/Public/{VERSION}/ucd/Unihan.zip"
SOURCE_SHA256 = "f7a48b2b545acfaa77b2d607ae28747404ce02baefee16396c5d2d7a8ef34b5e"
ROOT = Path(__file__).resolve().parents[1]


def syllable(text):
    # Preserve umlaut as v (lv/lve), drop tone marks.
    text = unicodedata.normalize("NFD", text.lower())
    text = text.replace("u\u0308", "v")
    return "".join(c for c in text if "a" <= c <= "z")


def main():
    cache = ROOT / ".local" / f"Unihan-{VERSION}.zip"
    cache.parent.mkdir(parents=True, exist_ok=True)
    archive = cache.read_bytes() if cache.exists() else urllib.request.urlopen(SOURCE).read()
    if hashlib.sha256(archive).hexdigest() != SOURCE_SHA256:
        raise ValueError("Unexpected Unihan archive hash")
    cache.write_bytes(archive)
    readings = {}
    with zipfile.ZipFile(io.BytesIO(archive)) as zipped:
        lines = zipped.read("Unihan_Readings.txt").decode("utf-8").splitlines()
        variants = zipped.read("Unihan_Variants.txt").decode("utf-8").splitlines()
    for line in lines:
        if not line or line.startswith("#"):
            continue
        code, prop, value = line.split("\t", 2)
        if prop not in ("kMandarin", "kHanyuPinyin"):
            continue
        values = value.split() if prop == "kMandarin" else [
            pinyin for entry in value.split() for pinyin in entry.split(":", 1)[1].split(",")
        ]
        char = chr(int(code[2:], 16))
        readings.setdefault(char, set()).update(filter(None, map(syllable, values)))
    # Several simplified entries only have their default reading (长=zhang, 乐=le).
    # Unihan's explicit simplified/traditional links supply the alternate readings.
    parents = {}

    def root(char):
        parents.setdefault(char, char)
        if parents[char] != char:
            parents[char] = root(parents[char])
        return parents[char]

    for line in variants:
        if not line or line.startswith("#"):
            continue
        code, prop, value = line.split("\t", 2)
        if prop not in ("kTraditionalVariant", "kSimplifiedVariant"):
            continue
        char = chr(int(code[2:], 16))
        for entry in value.split():
            other = chr(int(entry.split("<", 1)[0][2:], 16))
            parents[root(other)] = root(char)
    combined = {}
    for char in list(parents):
        combined.setdefault(root(char), set()).update(readings.get(char, ()))
    for char in parents:
        if combined[root(char)]:
            readings.setdefault(char, set()).update(combined[root(char)])
    output = ROOT / "src/BetterHotkeys/Data/pinyin.tsv"
    output.parent.mkdir(parents=True, exist_ok=True)
    header = f"# Unicode Unihan {VERSION}; kMandarin + kHanyuPinyin + simplified/traditional variants; toneless, ü=v\n# {SOURCE}\n# Source SHA256: {hashlib.sha256(archive).hexdigest()}\n# See THIRD-PARTY-NOTICES.txt for Unicode License V3.\n"
    output.write_text(header + "".join(f"{char}\t{','.join(sorted(values))}\n" for char, values in sorted(readings.items())), encoding="utf-8")
    license_text = urllib.request.urlopen("https://www.unicode.org/license.txt").read().decode("utf-8")
    (ROOT / "THIRD-PARTY-NOTICES.txt").write_text(
        f"Offline pinyin search data derived from Unicode Unihan {VERSION}.\n{SOURCE}\n"
        "Properties: kMandarin and kHanyuPinyin, merged via kSimplifiedVariant/kTraditionalVariant; tones removed, umlaut represented as v.\n"
        "Generator: scripts/Generate-Pinyin.py. Data is embedded in BetterHotkeys.dll.\n\n" + license_text,
        encoding="utf-8",
    )
    print(f"Wrote {len(readings)} characters, {output.stat().st_size} bytes; SHA256 {hashlib.sha256(archive).hexdigest()}")


if __name__ == "__main__":
    main()
