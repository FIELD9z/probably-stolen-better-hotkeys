using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BetterHotkeys
{
    // Offline, Unity-free name search. Multiple readings are matched without enumerating
    // every pronunciation combination of a name. The embedded table is shared across saves.
    internal sealed class CatalogSearch
    {
        private static readonly Lazy<Dictionary<string, string[]>> readings = new Lazy<Dictionary<string, string[]>>(LoadReadings);
        private static readonly Regex markup = new Regex("<[^>]+>");
        private readonly string name;
        private string[][]? tokens;

        internal CatalogSearch(string name) { this.name = Normalize(name); }

        internal static string Normalize(string value)
        {
            var text = markup.Replace(value ?? "", "").ToLowerInvariant().Normalize(NormalizationForm.FormD)
                .Replace("u\u0308", "v").Replace("u:", "v");
            var result = new StringBuilder();
            foreach (var c in text)
                if (char.IsLetterOrDigit(c) || char.IsSurrogate(c)) result.Append(c);
            return result.ToString();
        }

        internal bool Matches(string query)
        {
            query = Normalize(query);
            if (query.Length == 0 || name.IndexOf(query, StringComparison.Ordinal) >= 0) return true;
            if (tokens == null)
            {
                var parts = new List<string[]>();
                for (var i = 0; i < name.Length; i++)
                {
                    var glyph = char.IsHighSurrogate(name[i]) && i + 1 < name.Length && char.IsLowSurrogate(name[i + 1])
                        ? name.Substring(i++, 2) : name[i].ToString();
                    parts.Add(readings.Value.TryGetValue(glyph, out var sounds)
                        ? sounds.Concat(sounds.Select(sound => sound.Substring(0, 1))).Append(glyph).Distinct().ToArray()
                        : new[] { glyph });
                }
                tokens = parts.ToArray();
            }
            for (var start = 0; start < tokens.Length; start++)
            {
                var offsets = new HashSet<int> { 0 };
                for (var i = start; i < tokens.Length && offsets.Count != 0; i++)
                {
                    var next = new HashSet<int>();
                    foreach (var offset in offsets)
                        foreach (var form in tokens[i])
                        {
                            var length = Math.Min(form.Length, query.Length - offset);
                            if (string.CompareOrdinal(form, 0, query, offset, length) != 0) continue;
                            if (offset + length == query.Length) return true;
                            next.Add(offset + length);
                        }
                    offsets = next;
                }
            }
            return false;
        }

        private static Dictionary<string, string[]> LoadReadings()
        {
            var result = new Dictionary<string, string[]>(StringComparer.Ordinal);
            using var stream = typeof(CatalogSearch).Assembly.GetManifestResourceStream("BetterHotkeys.pinyin.tsv")
                ?? throw new InvalidOperationException("Missing embedded pinyin search data.");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                var fields = line.Split('\t');
                result.Add(fields[0], fields[1].Split(','));
            }
            return result;
        }
    }
}
