# Workshop release

Version: 0.5.5
Title: Better Hotkeys & Storage / 更好的快捷键与收纳
Author: FIELD9z
App: 4348910; internal game version: 0.46D
Existing Workshop item: https://steamcommunity.com/sharedfiles/filedetails/?id=3829363629

Descriptions: workshop-description.zh-CN.txt and workshop-description.en.txt.
Package: dist/BetterHotkeys (BetterHotkeys.dll, manifest.xml, USER-GUIDE.md and THIRD-PARTY-NOTICES.txt).
Release notes: release-0.5.5.md. Internal ID and configuration identities retained.
Source repository: https://github.com/FIELD9z/probably-stolen-better-hotkeys
Versioned source: https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v0.5.5
Keep both source links in each localized description when simplifying or updating it.
Local publication records reside under .local/workshop. Never create a replacement item for an update.

## Published 2026-11-01

- Updated existing item `3829363629` to 0.5.3; release-description commit `2ec0f45`.
- Steam SDK returned success for content and both localized descriptions. Public English and Simplified Chinese pages were then fetched and their rendered description text matched the approved local files.
- Native query confirmed public visibility, no ban, both localized titles, exact description matches, and total content size 631244 bytes, equal to the four-file package.
- DLL SHA256: `9C740DAD5AC1344DDFA9B8EC28C913C078967ECE1B8F5474972D5B01FBB9F570`, identical to the user-tested local installation.
- The localized title must be supplied together with its description: the description-only update returned an empty Chinese title; supplying the original Chinese title restored it. Final queries and public-page checks passed.
- Detailed receipts and verification snapshots: `.local/workshop/published-0.5.3.json`, `published-0.5.3-localization.json`, `queried-0.5.3.json`, `verified-0.5.3.json` and the language-specific HTML snapshots.


## Published 2026-11-07 — 0.5.5

- User tested the installed opposite-corner markers and approved this update. Updated existing Workshop item `3829363629`; no replacement item was created.
- Source commit: `5abe124c8e41205021ab92357a8652c5a269c8e3`; immutable tag `v0.5.5`. Release-description commit: `3612afe`. GitHub master and tag were read back before Workshop upload; the tag resolves to the installed source commit.
- Steam SDK reported success for the package and both localized titles/descriptions. Native readback confirmed public visibility, no ban, exact descriptions and 636254 bytes, equal to the four-file package.
- DLL SHA256: `3C9E605F44CA155F1EB79C9584C986D4CD7E0EFE9B6340FB9AD06387C06A1F3C`. All package hashes match the user-tested 0.5.5 local deployment record; no rebuild or binary change was introduced for publication.
- English and Simplified Chinese public pages initially returned an unavailable/hidden-item message after submission, then recovered on a later read. Final rendered text matches the approved local descriptions, including both source-link labels and actual targets:
  - Repository: https://github.com/FIELD9z/probably-stolen-better-hotkeys
  - Version: https://github.com/FIELD9z/probably-stolen-better-hotkeys/tree/v0.5.5
- Local evidence: `.local/workshop/published-0.5.5.json`, `queried-0.5.5.json`, `verified-0.5.5.json`, both public-page HTML snapshots and `package-0.5.5/`.
