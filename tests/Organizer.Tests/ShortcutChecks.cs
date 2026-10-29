using BetterHotkeys;

internal static class ShortcutChecks
{
    internal static void Run(Action<string, bool> check)
    {
        Shortcut Parse(string text) { if (!Shortcut.TryParse(text, out var result)) throw new Exception(text); return result; }
        check("blank binding clears action", Parse(" ").IsNone && Parse("none").IsNone);
        check("case-insensitive aliases", Parse("rightcontrol + q").Text == "RightCtrl+Q");
        check("modifier order normalized", Parse("Q + Alt + Ctrl").Text == "Ctrl+Alt+Q");
        foreach (var invalid in new[] { "Ctrl++Q", "A+B", "Shift+RightShift", "F8", "Ctrl+F9", "Mouse0", "NotAKey" })
            check("reject malformed/reserved " + invalid, !Shortcut.TryParse(invalid, out _));
        var shift = Parse("Shift");
        check("right shift equivalent enabled", shift.Matches(k => k == "Shift", true));
        check("right shift does not match left-only", !shift.Matches(k => k == "Shift" || k == "RightShift", false));
        check("explicit right shift matches", Parse("RightShift").Matches(k => k == "Shift" || k == "RightShift", false));
        check("extra modifier cancels plain key", !Parse("B").Matches(k => k == "B" || k == "Ctrl", true));
        check("bound combination matches", Parse("Ctrl+Q").Matches(k => k == "Ctrl" || k == "Q", true));
        check("combination fires on primary key press", Parse("Ctrl+Q").Pressed(k => k == "Ctrl" || k == "Q", k => k == "Q", true));
        check("combination does not fire on modifier press", !Parse("Ctrl+Q").Pressed(k => k == "Ctrl" || k == "Q", k => k == "Ctrl", true));
        check("cleared binding cannot fire", !Parse("None").Pressed(_ => true, _ => true, true));
        check("modifier and modified key overlap", shift.Overlaps(Parse("Shift+Q"), true));
        check("left/right alias overlap enabled", shift.Overlaps(Parse("RightShift"), true));
        check("left/right alias separate disabled", !shift.Overlaps(Parse("RightShift"), false));
        check("different modifier chords independent", !shift.Overlaps(Parse("Ctrl+Q"), true));
        check("different primary keys independent", !Parse("Ctrl+Q").Overlaps(Parse("Ctrl+B"), true));
        check("cleared binding never overlaps", !shift.Overlaps(Parse("None"), true));
    }
}
