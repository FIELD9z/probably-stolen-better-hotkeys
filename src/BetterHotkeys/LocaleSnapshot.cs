using System;
using System.Reflection;

namespace BetterHotkeys
{
    // Read only the game's already-created, completed locale selection. In particular, do not
    // call LocalizationSettings.Instance/SelectedLocale or LocHelper.GetCurrentLocaleCode:
    // those getters create settings, select a locale and can synchronously wait during Mod.Init.
    internal static class LocaleSnapshot
    {
        private static string lastCode = "en";
        private static FieldInfo? instanceField, selectionField, identifierField, codeField;
        private static MethodInfo? validMethod;
        private static PropertyInfo? doneProperty, resultProperty;

        internal static string Code
        {
            get
            {
                try
                {
                    if (instanceField == null && !BindLoadedMetadata()) return lastCode;
                    var instance = instanceField!.GetValue(null);
                    if (instance == null) return lastCode;
                    var handle = selectionField!.GetValue(instance);
                    // IsDone alone is insufficient: Unity considers an invalid handle done.
                    if (handle == null || !(bool)validMethod!.Invoke(handle, null)! ||
                        !(bool)doneProperty!.GetValue(handle)!) return lastCode;
                    var locale = resultProperty!.GetValue(handle);
                    if (locale == null) return lastCode;
                    var identifier = identifierField!.GetValue(locale);
                    var code = identifier == null ? null : codeField!.GetValue(identifier) as string;
                    if (!string.IsNullOrEmpty(code)) lastCode = code!;
                }
                catch (Exception)
                {
                    // A changed game API or an invalidated handle must not affect native localization.
                    // Preserve the last completed observation; explicit mod language still works.
                }
                return lastCode;
            }
        }

        private static bool BindLoadedMetadata()
        {
            // Never Assembly.Load: localization may legitimately not have been loaded at Mod.Init.
            Type? settings = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == "Unity.Localization")
                { settings = assembly.GetType("UnityEngine.Localization.Settings.LocalizationSettings", false); break; }
            if (settings == null) return false;
            const BindingFlags fields = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var instance = settings.GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            var selection = settings.GetField("m_SelectedLocaleAsync", fields);
            var handle = selection?.FieldType;
            var valid = handle?.GetMethod("IsValid", Type.EmptyTypes);
            var done = handle?.GetProperty("IsDone");
            var result = handle?.GetProperty("Result");
            var identifier = result?.PropertyType.GetField("m_Identifier", fields);
            var code = identifier?.FieldType.GetField("m_Code", fields);
            if (instance == null || selection == null || valid == null || done == null || result == null || identifier == null || code == null) return false;
            selectionField = selection; validMethod = valid; doneProperty = done; resultProperty = result;
            identifierField = identifier; codeField = code;
            instanceField = instance; // Publish only a complete binding.
            return true;
        }
    }
}
