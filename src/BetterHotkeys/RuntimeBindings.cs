using System;
using System.Linq;
using UnityEngine;

namespace BetterHotkeys
{
    internal static class RuntimeBindings
    {
        internal static bool Key(string name, bool down = false)
        {
            bool Read(KeyCode code) => down ? Input.GetKeyDown(code) : Input.GetKey(code);
            if (name == "Shift") return Read(KeyCode.LeftShift) || Read(KeyCode.RightShift);
            if (name == "Ctrl") return Read(KeyCode.LeftControl) || Read(KeyCode.RightControl);
            if (name == "Alt") return Read(KeyCode.LeftAlt) || Read(KeyCode.RightAlt);
            return Enum.TryParse<KeyCode>(name.Replace("Ctrl", "Control"), out var key) && Read(key);
        }
        internal static bool Held(Shortcut chord) => chord.Matches(name => Key(name), Settings.Current.Equivalent);
        internal static bool Down(Shortcut chord) => chord.Pressed(name => Key(name), name => Key(name, true), Settings.Current.Equivalent);
        private static bool Uses(Shortcut chord, KeyCode key)
        {
            var name = key.ToString().Replace("Control", "Ctrl");
            return chord.Keys.Any(k => Shortcut.Canonical(k, true) == Shortcut.Canonical(name, true));
        }
        internal static string Conflict(Shortcut chord, bool sorting = false)
        {
            if (chord.IsNone) return "";
            if (Uses(chord, KeyCode.T)) return "SmoothMouseMovement (T)";
            foreach (var binding in KeyBindHelper.Bindings)
            {
                if (Settings.OwnsInput && binding.id.StartsWith("quick_move_", StringComparison.Ordinal)) continue;
                if (sorting && binding.id == "sort_menu") continue;
                if (Uses(chord, KeyBindHelper.Get(binding.id))) return binding.id;
            }
            var coordinator = OrganizerWorld.InStore ? MachineStorageCoordinator.FindCoordinatorInReach() : null;
            if (coordinator != null)
                foreach (var preset in MachineStorageCoordinator.LoadPresets(coordinator))
                    if (Uses(chord, (KeyCode)preset.keyCode)) return preset.name;
            return "";
        }
        internal static bool Allowed(Shortcut chord, bool sorting = false, bool notify = true)
        {
            var conflict = Conflict(chord, sorting);
            if (conflict.Length == 0) return true;
            if (notify) OrganizerInput.Notice(Texts.T($"按键 {chord.Text} 与 {conflict} 冲突，未执行。", $"Binding {chord.Text} conflicts with {conflict}; action skipped."));
            return false;
        }
    }
}
