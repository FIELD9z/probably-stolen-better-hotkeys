param([string]$GameDir)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $GameDir) {
    [xml]$config = Get-Content (Join-Path $root '.local/Game.props') -Raw
    $GameDir = [string]$config.Project.PropertyGroup.GameDir
}
$managed = Join-Path $GameDir 'Probably Stolen_Data/Managed'
[void][Reflection.Assembly]::LoadFrom((Join-Path $managed 'Unity.Burst.Cecil.dll'))
$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $managed 'Assembly-CSharp.dll'))
$mod = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $root 'src/StarterMod/bin/Release/netstandard2.1/StarterMod.dll'))
function Calls($assembly, $type, $method) {
    $definition = $assembly.MainModule.Types | Where-Object FullName -EQ $type
    $body = $definition.Methods | Where-Object Name -EQ $method
    if (@($body).Count -ne 1) { throw "Expected one method: $type.$method" }
    @($body.Body.Instructions | Where-Object { $_.OpCode.Name -in @('call','callvirt') } | ForEach-Object { $_.Operand.ToString() })
}
try {
    # Compare bindings with actual sale actions, not English field-name guesses.
    foreach ($entry in @(
        @($game, 'PlayerStore', 'SellItemFromTable'),
        @($game, 'PlayerStore', 'SellAllItemFromTable'),
        @($mod, 'StarterMod.OrganizerWorld', 'Classify'),
        @($mod, 'StarterMod.OrganizerInput', 'Transfer')
    )) {
        $calls = Calls $entry[0] $entry[1] $entry[2]
        if (-not ($calls -match 'EmporiumEntry::get_backInvinvElement\(\)')) { throw "Sale-grid binding missing: $($entry[1]).$($entry[2])" }
        if ($calls -match 'EmporiumEntry::get_(frontInvinvElement|showcaseElement)\(\)') { throw "Wrong counter binding: $($entry[1]).$($entry[2])" }
    }
    foreach ($type in @('ItemQuickTransferHandler','ItemQuickTransferAltHandler','ItemQuickTransferAltHandler1')) {
        $definition = $game.MainModule.Types | Where-Object Name -EQ $type
        if ($definition.BaseType.FullName -ne 'EmptyActionHandler') { throw "Patch instance type mismatch: $type" }
        foreach ($method in @('OnEventPress','OnEventRelease','OnEventUpdate','OnEventReset')) {
            if (-not ($definition.Methods.Name -contains $method)) { throw "Missing quick-transfer entry: $type.$method" }
        }
    }
    foreach ($type in @('StarterMod.QuickTransferEventPatch','StarterMod.QuickTransferUpdatePatch')) {
        $calls = Calls $mod $type 'Prefix'
        if (-not ($calls -match 'Settings::get_OwnsInput') -or -not ($calls -match 'UpdateReset')) { throw "Missing input ownership guard/reset: $type" }
        if ($calls -match 'OrganizerWorld|OrganizerInput') { throw "Location-dependent native fallback: $type" }
    }
    $ownership = Calls $mod 'StarterMod.Settings' 'get_OwnsInput'
    if (-not ($ownership -match 'ModEntry::get_Active') -or -not ($ownership -match 'Settings::get_Current')) { throw 'Input ownership must require an enabled mod and runtime preferences.' }
    # Maintenance package must not contain development features or native hint-panel mutation.
    if ($mod.MainModule.Types.FullName -match 'StarterMod\.(CustomerDemandOverlay|AutoStorage|StorageEditor|ControlHints|ControlHintRefreshPatch)$') {
        throw 'Development feature or native hint replacement leaked into the maintenance build.'
    }
    foreach ($type in $mod.MainModule.Types) {
        foreach ($method in $type.Methods) {
            if ($method.HasBody -and ($method.Body.Instructions.Operand -match 'StoreUIManager::controlHintOverlay')) {
                throw "Native shortcut overlay access remains: $($type.FullName).$($method.Name)"
            }
        }
    }
    Write-Output 'Game binding checks passed. Runtime Harmony dispatch and UI behavior still require in-game testing.'
} finally {
    $game.Dispose()
    $mod.Dispose()
}
