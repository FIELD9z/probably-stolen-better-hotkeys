[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
[xml]$config = Get-Content (Join-Path $repoRoot '.local/Game.props') -Raw
$gameDir = [IO.Path]::GetFullPath([string]$config.Project.PropertyGroup.GameDir)
$destination = Join-Path $gameDir 'Mods/BetterHotkeys'
$package = Join-Path $repoRoot 'dist/BetterHotkeys'
function AssertGameClosed {
    if (Get-Process | Where-Object { $_.ProcessName -match 'Probably.*Stolen' }) {
        throw 'Probably Stolen is running. Exit the game before installation; no files were replaced.'
    }
}
function FileHashes([string]$directory) {
    $hashes = [ordered]@{}
    if (Test-Path -LiteralPath $directory) {
        Get-ChildItem -LiteralPath $directory -File -Recurse | Sort-Object FullName | ForEach-Object {
            $hashes[[IO.Path]::GetRelativePath($directory, $_.FullName)] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        }
    }
    return $hashes
}
AssertGameClosed
$dirty = & git -C $repoRoot status --porcelain -- src scripts tests docs/user-guide.md docs/research.md THIRD-PARTY-NOTICES.txt
if ($LASTEXITCODE -ne 0 -or $dirty) { throw 'Commit verified code, tests, scripts and packaged documentation before installing.' }
$commit = (& git -C $repoRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve installation commit.' }
[xml]$manifest = Get-Content (Join-Path $package 'manifest.xml') -Raw
[xml]$sourceManifest = Get-Content (Join-Path $repoRoot 'src/BetterHotkeys/manifest.xml') -Raw
if ($manifest.Manifest.ModVersion -ne $sourceManifest.Manifest.ModVersion) { throw 'Package version is stale; rebuild.' }
foreach ($pair in @(
    @('src/BetterHotkeys/bin/Release/netstandard2.1/BetterHotkeys.dll', 'BetterHotkeys.dll'),
    @('docs/user-guide.md', 'USER-GUIDE.md'),
    @('THIRD-PARTY-NOTICES.txt', 'THIRD-PARTY-NOTICES.txt')
)) {
    if ((Get-FileHash -LiteralPath (Join-Path $repoRoot $pair[0])).Hash -ne (Get-FileHash -LiteralPath (Join-Path $package $pair[1])).Hash) { throw "Package file is stale: $($pair[1])" }
}
$before = FileHashes $destination
$expected = FileHashes $package
$backup = Join-Path $repoRoot ('.local/deploy-backups/' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + $commit.Substring(0, 8))
New-Item -ItemType Directory -Path $backup | Out-Null
if (Test-Path -LiteralPath $destination) {
    Copy-Item -LiteralPath $destination -Destination (Join-Path $backup 'installed') -Recurse
    $copied = FileHashes (Join-Path $backup 'installed')
    if ($copied.Count -ne $before.Count) { throw 'Incomplete installation backup.' }
    foreach ($key in $before.Keys) { if ($copied[$key] -ne $before[$key]) { throw "Backup hash mismatch: $key" } }
}
$record = [ordered]@{ commit = $commit; version = [string]$manifest.Manifest.ModVersion; gameVersion = [string]$manifest.Manifest.GameVersions.Version; installedAt = [DateTimeOffset]::Now.ToString('o'); destination = $destination; backup = $backup; previousHashes = $before; packageHashes = $expected; status = 'backed-up' }
$recordPath = Join-Path $backup 'deployment.json'
$record | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $recordPath -Encoding utf8
AssertGameClosed
New-Item -ItemType Directory -Force -Path $destination | Out-Null
Get-ChildItem -LiteralPath $package | Copy-Item -Destination $destination -Recurse -Force
$after = FileHashes $destination
foreach ($key in $expected.Keys) { if ($after[$key] -ne $expected[$key]) { throw "Installed hash mismatch: $key. Backup: $backup" } }
$record.status = 'installed-and-verified'
$record | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $recordPath -Encoding utf8
Write-Output "Installed $($record.version) from commit $commit; package hashes verified. Backup: $backup"
