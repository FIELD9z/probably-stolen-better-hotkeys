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
                return LocaleSnapshot.Code.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            }
        }
        internal static string T(string zh, string en) => Chinese ? zh : en;
        internal static string ColorName(string name)
        {
            switch (name)
            {
                case "Yellow": return T("黄色", "Yellow");
                case "Red": return T("红色", "Red");
                case "Purple": return T("紫色", "Purple");
                case "Cyan": return T("青色", "Cyan");
                case "Orange": return T("橙色", "Orange");
                case "White": return T("白色", "White");
                case "Green": return T("绿色", "Green");
                default: return name;
            }
        }
        internal static string Name => T("更好的快捷键与收纳", "Better Hotkeys & Storage");
    }
}
