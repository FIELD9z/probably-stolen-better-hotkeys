using System;
using System.Collections.Generic;

namespace BetterHotkeys
{
    internal sealed class StorageTarget
    {
        internal GameItem Owner { get; }
        internal int Page { get; }
        internal GameGridInventory Grid { get; }
        internal PixelWindow Window { get; }
        internal string Key => Owner.uniqueId == 0 ? "" : Owner.uniqueId + ":" + Page;
        internal StorageTarget(GameItem owner, int page, GameGridInventory grid, PixelWindow window)
        { Owner = owner; Page = page; Grid = grid; Window = window; }
    }

    internal static class StorageTargets
    {
        private static bool Owned(GameItem item) => !item.isDebugMenu &&
            !(item is GameItemElement element && element.IsDestroyed()) &&
            GeneralHelper.IsItemOwned(item) && !GeneralHelper.IsInsideNonOwnedContainer(item) &&
            !item.IsInsideContainerPlacedForSelling();

        private static MachineryTabs? Tabs(PixelWindow window) =>
            (window.childElement as MachineryPanelRow)?.GetPanel("storage")?.content as MachineryTabs;

        private static IEnumerable<StorageTarget> Pages(GameItem owner)
        {
            var window = owner.contentWindow;
            if (window == null || !Owned(owner) || window.isLocked) yield break;
            if (window.childElement is GameGridInventory direct)
            { yield return new StorageTarget(owner, 0, direct, window); yield break; }
            // Restrict this wrapper to the verified expansion implementation, not arbitrary machinery panels.
            var id = owner.identifier;
            if (id != "storage_bay" && id != "storage_bay_gun" && id != "storage_bay_chem" && id != "storage_bay_large") yield break;
            var tabs = Tabs(window);
            if (tabs == null) yield break;
            for (var i = 0; i < tabs.count; i++)
                if ((tabs.GetPage(i) as MachineryIntake)?.content is GameGridInventory grid)
                    yield return new StorageTarget(owner, i, grid, window);
        }

        internal static List<StorageTarget> Enumerate(bool openedOnly)
        {
            var result = new List<StorageTarget>();
            if (!OrganizerWorld.InStore) return result;
            var seen = new HashSet<GameInventory>();
            void Visit(GameInventory inventory)
            {
                if (!seen.Add(inventory)) return;
                foreach (var item in inventory.childItems)
                    foreach (var target in Pages(item))
                    {
                        var tabs = Tabs(target.Window);
                        if (!openedOnly || (target.Window.IsVisible() && target.Window.handler.gameObject.activeInHierarchy && (tabs == null || tabs.selected == target.Page)))
                            result.Add(target);
                        // Only traverse supported owned storage: machine intakes and sale roots never enter the walk.
                        Visit(target.Grid);
                    }
            }
            Visit(EmporiumEntry.Instance.invElement);
            return result;
        }

        internal static StorageTarget? FromWindow(PixelWindow window)
        {
            foreach (var target in Enumerate(true)) if (ReferenceEquals(target.Window, window)) return target;
            return null;
        }

        internal static bool IsEligible(StorageTarget target)
        {
            foreach (var current in Enumerate(false))
                if (ReferenceEquals(current.Owner, target.Owner) && current.Page == target.Page && ReferenceEquals(current.Grid, target.Grid))
                    return !target.Grid.IsInsertLocked() && GeneralHelper.MayPlayerInsertInto(target.Grid);
            return false;
        }
    }
}
