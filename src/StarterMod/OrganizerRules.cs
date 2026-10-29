using System;

namespace StarterMod
{
    internal enum Place { Unsupported, Inventory, Counter, Container }
    [Flags]
    internal enum Modifier { None = 0, Shift = 1, Ctrl = 2, Alt = 4 }
    internal enum GestureResult { Pending, Transfer, Pin, Drag, Cancel }

    // No Unity dependencies: the agreed routes and click/drag boundary are testable outside the game.
    internal static class OrganizerRules
    {
        internal static Place Destination(Place source, Modifier modifier)
        {
            if (modifier == Modifier.Shift)
                return source == Place.Counter ? Place.Inventory :
                    source == Place.Inventory || source == Place.Container ? Place.Counter : Place.Unsupported;
            if (modifier == Modifier.Ctrl)
                return source == Place.Inventory ? Place.Container :
                    source == Place.Container ? Place.Inventory : Place.Unsupported;
            return Place.Unsupported;
        }

        internal static bool IsSingle(Modifier modifier) =>
            modifier == Modifier.Shift || modifier == Modifier.Ctrl || modifier == Modifier.Alt;

        internal static GestureResult Evaluate(Modifier initial, Modifier current, bool cancelled,
            bool crossedThreshold, bool released, bool sameItem)
        {
            if (cancelled || !IsSingle(initial) || current != initial)
                return GestureResult.Cancel;
            if (crossedThreshold)
                return initial == Modifier.Alt ? GestureResult.Cancel : GestureResult.Drag;
            if (!released) return GestureResult.Pending;
            if (!sameItem) return GestureResult.Cancel;
            return initial == Modifier.Alt ? GestureResult.Pin : GestureResult.Transfer;
        }
    }
}
