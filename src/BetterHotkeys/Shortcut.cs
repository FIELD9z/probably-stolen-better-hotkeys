using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterHotkeys
{
    // Unity-free grammar shared by validation and tests. Mouse gestures use this held chord + left click.
    internal sealed class Shortcut
    {
        internal string Text { get; }
        internal string[] Keys { get; }
        internal bool IsNone => Keys.Length == 0;
        private Shortcut(string text, string[] keys) { Text = text; Keys = keys; }
        internal static int Family(string key) => key.Contains("Shift") ? 1 : key.Contains("Ctrl") ? 2 : key.Contains("Alt") ? 4 : 0;
        internal static string Canonical(string key, bool equivalent)
        {
            if (Family(key) == 0) return key;
            var name = Family(key) == 1 ? "Shift" : Family(key) == 2 ? "Ctrl" : "Alt";
            return equivalent ? name : (key.StartsWith("Right", StringComparison.Ordinal) ? "Right" : "Left") + name;
        }
        internal string Signature(bool equivalent) => string.Join("+", Keys.Select(k => Canonical(k, equivalent)).OrderBy(k => k));
        internal bool Overlaps(Shortcut other, bool equivalent)
        {
            if (IsNone || other.IsNone) return false;
            var modifiers = Keys.Where(k => Family(k) != 0).Select(k => Canonical(k, equivalent)).OrderBy(k => k);
            var otherModifiers = other.Keys.Where(k => Family(k) != 0).Select(k => Canonical(k, equivalent)).OrderBy(k => k);
            if (!modifiers.SequenceEqual(otherModifiers)) return false;
            var key = Keys.FirstOrDefault(k => Family(k) == 0);
            var otherKey = other.Keys.FirstOrDefault(k => Family(k) == 0);
            return key == null || otherKey == null || key == otherKey;
        }
        internal bool Matches(Func<string, bool> held, bool equivalent)
        {
            if (IsNone) return false;
            foreach (var key in Keys)
            {
                var normalized = Canonical(key, equivalent);
                if (!held(normalized)) return false;
            }
            var families = Keys.Aggregate(0, (mask, key) => mask | Family(key));
            foreach (var family in new[] { "Shift", "Ctrl", "Alt" })
                if (held(family) != ((families & Family(family)) != 0)) return false;
            return true;
        }
        internal bool Pressed(Func<string, bool> held, Func<string, bool> down, bool equivalent) =>
            Matches(held, equivalent) && down(Canonical(Keys.Last(), equivalent));

        internal static bool TryParse(string text, out Shortcut shortcut)
        {
            shortcut = new Shortcut("None", Array.Empty<string>());
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Equals("None", StringComparison.OrdinalIgnoreCase)) return true;
            var allowed = Enumerable.Range('A', 26).Select(i => ((char)i).ToString())
                .Concat(Enumerable.Range(1, 12).Select(i => "F" + i))
                .Concat(new[] { "Shift", "Ctrl", "Alt", "LeftShift", "RightShift", "LeftCtrl", "RightCtrl", "LeftAlt", "RightAlt", "Space", "Tab", "BackQuote", "Home", "End", "PageUp", "PageDown", "Insert" }).ToArray();
            var keys = new List<string>();
            foreach (var token in text.Split('+'))
            {
                var input = token.Trim();
                if (input.EndsWith("Control", StringComparison.OrdinalIgnoreCase)) input = input.Substring(0, input.Length - 7) + "Ctrl";
                var key = allowed.FirstOrDefault(k => k.Equals(input, StringComparison.OrdinalIgnoreCase));
                if (key == null || key == "F8" || key == "F9" || keys.Any(k => k == key || (Family(k) != 0 && Family(k) == Family(key)))) return false;
                keys.Add(key);
            }
            if (keys.Count(k => Family(k) == 0) > 1) return false;
            keys = keys.OrderBy(k => Family(k) == 0 ? 10 : Family(k)).ToList();
            shortcut = new Shortcut(string.Join("+", keys), keys.ToArray());
            return true;
        }
    }
}
