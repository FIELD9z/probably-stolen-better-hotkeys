[CmdletBinding()]
param(
    [string]$GameDir,
    [string]$GameVersion
)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repoRoot 'src/StarterMod/StarterMod.csproj'
$buildArgs = @('build', $project, '-c', 'Release', '--nologo')
if ($GameDir) { $buildArgs += "-p:GameDir=$GameDir" }
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
if (-not $GameVersion) {
    Write-Host 'Build passed. To package, pass -GameVersion with the actual in-game version (not the Steam Build ID).'
    return
}
if ($GameVersion -eq 'REPLACE_WITH_GAME_VERSION') { throw 'Supply the actual in-game version.' }
[xml]$manifest = Get-Content (Join-Path $repoRoot 'src/StarterMod/manifest.xml') -Raw
$manifest.Manifest.GameVersions.Version = $GameVersion
$package = Join-Path $repoRoot 'dist/StarterMod'
New-Item -ItemType Directory -Force -Path $package | Out-Null
Copy-Item -LiteralPath (Join-Path $repoRoot 'src/StarterMod/bin/Release/netstandard2.1/StarterMod.dll') -Destination $package
$manifest.Save((Join-Path $package 'manifest.xml'))
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs/user-guide.md') -Destination (Join-Path $package 'USER-GUIDE.md')
Write-Host "Package created: $package"
