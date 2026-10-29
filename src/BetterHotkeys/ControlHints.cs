using System;
using HarmonyLib;
namespace BetterHotkeys
{
    // Independent mod help only. No native controlHintOverlay access or mutation.
    internal static class ControlHints
    {
        private static CustomUIWindow? help;
        internal static bool IsOpen => help != null && help.IsAlive && help.IsVisible;
        internal static void Restore() { help?.Close(); help = null; }
        internal static void Open(string layer)
        {
            Restore();
            var b = CustomUIManager.Window(Settings.Id + ":help", Texts.T("Better Hotkeys — 快捷键说明", "Better Hotkeys — shortcut guide"), layer)
                .SetSize(620, 560).Center().SetCloseOnEscape(true);
            help = b.GetWindow();
            b.BeginScroll(440);
            b.AddLabel(Texts.T("以下为已生效的 Mod 设置。修改键位后请应用并重新打开本说明。原生快捷键提示保持原样。", "These are the applied mod settings. Apply changes and reopen this guide to refresh. Native shortcut hints are unchanged."));
            if (!ModEntry.Active) b.AddWarning(Texts.T("Mod 当前未启用，以下仅为已保存配置说明。", "The mod is currently disabled; these describe saved settings only."));
            var rows = ControlHintRows.Build(true, Settings.Current, Texts.Chinese,
                id => KeyBindHelper.KeyName(KeyBindHelper.Get(id)), (key, sorting) => RuntimeBindings.Conflict(key, sorting));
            // Mod actions are the first five and last two rows; vanilla rotation/dialogue hints stay native.
            for (var i = 0; i < rows.Count; i++)
                if (i < 5 || i >= rows.Count - 2) b.AddLabel(rows[i].Key + " — " + rows[i].Action);
            b.AddLabel(Texts.T("容器标题栏设置图标：名称、标签、允许／拒绝物品。点选物品时 Esc／右键结束；虚影不占格子。", "Container header settings: names, tags, allowed/denied items. Esc/right-click ends item picking; previews use no inventory space."));
            b.End(); b.AddButton(Texts.T("关闭", "Close"), Restore); b.Show();
        }
    }
    [HarmonyPatch(typeof(ModConfigWindow), "Open")]
    internal static class ModHelpEntryPatch
    {
        private static void Postfix(LoadedMod __0, CustomUIWindow __result)
        {
            if (__0?.Manifest?.ID != Settings.Id || __result == null || !__result.IsAlive) return;
            try
            {
                var b = new CustomUIBuilder(CustomUIManager.Instance, __result, __result.ContentRoot);
                b.AddButton(Texts.T("Better Hotkeys 快捷键说明", "Better Hotkeys shortcut guide"), () => ControlHints.Open(__result.layerId), "better_hotkeys_help");
            }
            catch (Exception error) { ModEntry.Log?.Warning("Cannot add mod help entry: " + error.Message); }
        }
    }
}
