[CmdletBinding()]
param([string]$GameDir)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not $GameDir) {
    [xml]$props = Get-Content (Join-Path $repoRoot '.local/Game.props') -Raw
    $GameDir = $props.Project.PropertyGroup.GameDir
}
$managed = Join-Path $GameDir 'Probably Stolen_Data/Managed'
[void][Reflection.Assembly]::LoadFrom((Join-Path $managed 'Unity.Burst.Cecil.dll'))
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $managed 'Assembly-CSharp.dll'))
$checks = 0
function RequireMember([string]$typeName, [string]$member, [string]$kind) {
    $type = $assembly.MainModule.Types | Where-Object Name -EQ $typeName
    $members = if ($kind -eq 'Field') { $type.Fields } else { $type.Methods }
    if (-not ($members | Where-Object Name -EQ $member)) { throw "Missing $kind $typeName::$member" }
    $script:checks++
}
$access = Get-Content (Join-Path $repoRoot 'src/StarterMod/GameAccess.cs') -Raw
foreach ($match in [regex]::Matches($access, '(Field|Method)\(typeof\((\w+)\), "([^"]+)"\)')) {
    RequireMember $match.Groups[2].Value $match.Groups[3].Value $match.Groups[1].Value
}
foreach ($type in @('ItemQuickTransferHandler','ItemQuickTransferAltHandler','ItemQuickTransferAltHandler1')) {
    foreach ($method in @('OnEventPress','OnEventRelease','OnEventUpdate')) { RequireMember $type $method 'Method' }
    RequireMember $type 'isQuickTransfer' 'Field'
}
foreach ($method in @('OnEventPress','TriggerButton')) { RequireMember 'ItemContextHandler' $method 'Method' }
foreach ($field in @('currentInventory','canSort','buttonSortBigStart','buttonSortSmallStart','buttonSortBigEnd','buttonSortSmallEnd')) {
    RequireMember 'ItemContextHandler' $field 'Field'
}
RequireMember 'InputActionManager' 'Update' 'Method'
$assembly.Dispose()
Write-Host "$checks version-sensitive members verified without executing game code."
Get-FileHash (Join-Path $managed 'Assembly-CSharp.dll') -Algorithm SHA256 | Select-Object Algorithm, Hash
