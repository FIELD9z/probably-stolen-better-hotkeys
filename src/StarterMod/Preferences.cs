namespace StarterMod
{
    // Shared by input handling and the hint renderer; no UI-specific copy of bindings.
    internal sealed class Preferences
    {
        internal bool Takeover = true, Transfer = true, Pin = true, Sort = true, Markers = true, Equivalent = true;
        internal bool Batch = true, HeaderOnly, TargetBorder = true, TargetNotice = true, FollowSort = true, RememberSort = true;
        internal bool BigFirst = true, FromEnd, Stolen = true, Contraband = true, Nested = true, OwnedOnly, MarkerNotice = true;
        internal string Trigger = "DoublePress", Notices = "Exceptions", StolenColor = "Yellow", ContrabandColor = "Red", BothColor = "Purple";
        internal float DoubleInterval = .35f, Opacity = 1f, Refresh = .2f;
        internal int Width = 2;
        internal Shortcut Counter = Parse("Shift"), Container = Parse("Ctrl"), PinKey = Parse("Alt"), MarkerKey = Parse("B"), SortKey = Parse("S");
        internal bool DemandEnabled = true, AutoStore = true, StoreAllPages = false;
        internal bool EmptyStorageFallback = false, ForceManualStorage = false;
        internal string DemandColor = "Cyan";
        internal Shortcut DemandKey = Parse("D"), StoreKey = Parse("C");
        internal static Shortcut Parse(string key) { Shortcut.TryParse(key, out var result); return result; }
    }
}
