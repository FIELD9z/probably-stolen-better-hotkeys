[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
& dotnet run --project (Join-Path $repoRoot 'tests/Organizer.Tests/Organizer.Tests.csproj') -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Organizer tests failed: $LASTEXITCODE" }
