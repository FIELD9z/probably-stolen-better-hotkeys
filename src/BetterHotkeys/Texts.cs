using System;

namespace BetterHotkeys
{
    internal static class Texts
    {
        internal static bool Chinese
        {
            get
            {
                var language = Settings.Language;
                if (language != "Auto") return language == "简体中文";
                return (LocHelper.GetCurrentLocaleCode() ?? "en").StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            }
        }
        internal static string T(string zh, string en) => Chinese ? zh : en;
        internal static string Name => T("更好的快捷键", "Better Hotkeys");
    }
}
