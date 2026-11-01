using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterHotkeys
{
    internal static class StorageItemCatalog
    {
        private const string Prefix = "storage-catalog-v1:";
        private static float nextObserve;
        internal static void Reset() { nextObserve = 0; StorageItemDisplay.Reset(); }

        internal static void Tick()
        {
            if (Time.unscaledTime < nextObserve || !OrganizerWorld.InStore) return;
            nextObserve = Time.unscaledTime + 1f;
            ObserveVisible();
        }
        internal static void ObserveVisible()
        {
            if (!OrganizerWorld.InStore || WindowsHandler.current == null) return;
            var seen = new HashSet<GraphNodeStorage>();
            void Visit(GraphNodeStorage node)
            {
                if (!seen.Add(node)) return;
                if (node is GameItem)
                {
                    if (node is GameItemElement item && !item.IsDestroyed() && !item.isDebugMenu &&
                        item.middle != null && item.middle.gameObject.activeInHierarchy &&
                        item.handler?.FindParentWindow()?.IsVisible() == true &&
                        GeneralHelper.IsItemOwned(item) && !GeneralHelper.IsInsideNonOwnedContainer(item)) Observe(item);
                    return; // A displayed box does not disclose its closed contents. Open windows have their own roots.
                }
                foreach (var child in node.children) if (child != null) Visit(child);
            }
            foreach (var window in WindowsHandler.current.visibleWindows)
                if (window != null && window.IsVisible() && window.handler != null && window.handler.gameObject.activeInHierarchy) Visit(window);
        }
        internal static void Observe(GameItem item)
        {
            if (PlayerStore.Instance == null || string.IsNullOrEmpty(item.identifier)) return;
            StorageItemDisplay.Observe(item);
            var categories = string.Join(",", TypeHelper.MAJOR_TYPES_LIST.Where(type => item.IsGameItemType(type)));
            if (ModHelper.GetModData(Settings.Id, Prefix + item.identifier, "\0") != categories)
                ModHelper.SetModData(Settings.Id, Prefix + item.identifier, categories);
        }
        internal static List<CatalogOption> Options(IEnumerable<string> existing)
        {
            ObserveVisible();
            var result = new List<CatalogOption>();
            var keys = PlayerStore.Instance == null ? new List<string>() : ModHelper.GetModDataKeys(Settings.Id);
            // Entries already observed by the old picker also remain available after upgrading.
            var ids = keys.Where(k => k.StartsWith(Prefix, StringComparison.Ordinal)).Select(k => k.Substring(Prefix.Length))
                .Concat(StorageItemDisplay.KnownIds()).Concat(existing).Distinct(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                var types = ModHelper.GetModData(Settings.Id, Prefix + id, "").Split(',')
                    .Where(type => TypeHelper.MAJOR_TYPES_LIST.Contains(type)).Select(TypeHelper.GetTypeDisplayName).ToArray();
                result.Add(new CatalogOption(id, StorageItemDisplay.Name(id), types.Length == 0 ? new[] { Texts.T("其他 / 类别未记录", "Other / category not recorded") } : types));
            }
            return result;
        }
    }
}
