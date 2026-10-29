using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace StarterMod
{
    // Predicates verified against the local full-release IL. Never stage a candidate on the counter.
    internal static class CustomerDemandOverlay
    {
        private static readonly Dictionary<GameItemElement, DemandFrame> frames = new Dictionary<GameItemElement, DemandFrame>();
        private static StoreClient? lastClient;
        private static string? lastUnsupported;
        private static float nextRefresh;
        internal static void Reset() { Clear(); lastClient = null; lastUnsupported = null; nextRefresh = 0; }
        internal static void ConfigurationChanged() => Reset();
        private static void Clear() { foreach (var frame in frames.Values) frame.Dispose(); frames.Clear(); }

        internal static void Tick(bool blocked)
        {
            if (!Settings.Current.DemandEnabled || !OrganizerWorld.InStore) { Reset(); return; }
            var store = PlayerStore.Instance;
            var client = store.isClientArrived ? store.currentClientInstance?.GetClientBlueprint() : null;
            if (client == null || client.isBusinessComplete) { Reset(); return; }
            if (!ReferenceEquals(client, lastClient)) { Reset(); lastClient = client; }
            foreach (var pair in frames)
                if (!pair.Key.IsDestroyed() && pair.Value.Alive) pair.Value.Geometry(pair.Key.modifiedShape);
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + Settings.Current.Refresh;
            var match = Matcher(client, out var unsupported);
            if (unsupported != null && unsupported != lastUnsupported)
            {
                lastUnsupported = unsupported;
                OrganizerInput.Notice(Texts.T("当前顾客的特殊需求无法识别，未显示需求高亮。", "This customer's special request cannot be identified; demand markers are unavailable."));
                ModEntry.Log?.Log("Unsupported customer demand " + client.identifier + ": " + unsupported);
            }
            if (match == null) { Clear(); return; }
            Refresh(match);
        }

        private static Func<GameItem, bool>? Matcher(StoreClient client, out string? unsupported)
        {
            unsupported = null;
            if (client.gunOrderData != null)
            {
                var order = client.gunOrderData;
                // Service orders accept the customer's exact instance, not another equivalent gun.
                var serviceGun = order.isServiceOrder ? GameAccess.Field(typeof(GunOrderData), "serviceGun").GetValue(order) as GameItem : null;
                return item => (!order.isServiceOrder || ReferenceEquals(item, serviceGun)) &&
                    item.IsTag("ADV_GUN_TAG") && order.mandatoryRequirements.All(requirement => requirement.IsMet(item));
            }
            if (client.barterOffer != null)
            {
                var offer = client.barterOffer;
                if (offer.isFreeReward) return null;
                if (offer.UseCondition()) return item => offer.conditions.Any(condition => condition.IsItemValid(item));
                return ClosureMatcher(offer.getBarterResult, out unsupported);
            }
            if (client.procurementData != null) return ClosureMatcher(client.procurementData.getBarterResult, out unsupported);
            return client.IsClientBuying() ? new Func<GameItem, bool>(client.IsClientBuyingThisItem) : null;
        }

        private static Func<GameItem, bool>? ClosureMatcher(Func<BarterResult>? predicate, out string? unsupported)
        {
            // Exact native callback identity avoids guessing from dialogue or customer display names.
            // These highlight contributing items; they do not claim the whole order's quantity is met.
            var key = predicate?.Method.DeclaringType?.FullName + "::" + predicate?.Method.Name;
            unsupported = null;
            switch (key)
            {
                case "StoreClientListBarter+<>c::<CreateAddictBlood>b__3_1": return item => item.identifier == "pink_injector";
                case "StoreClientListBarter+<>c::<CreateScavBarter>b__4_1": return item => item.identifier == "moonshine";
                case "StoreClientListBarter+<>c::<CreateLowerLevelChemistBarter>b__6_1":
                case "StoreClientListUnused+<>c::<CreateBilly1>b__0_3": return item => item.IsTag("FOOD");
                case "StoreClientListProcurement+<>c::<CreateNurseProcurementOffer>b__0_0": return item => item.identifier == "blood_bag";
                case "StoreClientListProcurement+<>c::<CreateMachinistProcurementOffer>b__2_0": return item => item.identifier == "metal_ingot";
                default: unsupported = key; return null;
            }
        }

        private static void Refresh(Func<GameItem, bool> match)
        {
            var windows = WindowsHandler.current?.visibleWindows;
            if (windows == null) { Clear(); return; }
            var roots = windows.Where(window => window != null && window.IsVisible()).Cast<GraphNodeStorage>().ToList();
            // Native ordinary-buy matching recursively inspects container children. Exclude any node
            // reaching a cycle before invoking it; a managed StackOverflow cannot be recovered.
            var children = new Dictionary<GraphNodeStorage, HashSet<GraphNodeStorage>>();
            var parents = new Dictionary<GraphNodeStorage, HashSet<GraphNodeStorage>>();
            var queue = new Queue<GraphNodeStorage>(roots);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (children.ContainsKey(node)) continue;
                var links = new HashSet<GraphNodeStorage>(node.children.Where(child => child != null));
                children[node] = links;
                foreach (var child in links)
                {
                    if (!parents.TryGetValue(child, out var owners)) parents[child] = owners = new HashSet<GraphNodeStorage>();
                    owners.Add(node);
                    if (!children.ContainsKey(child)) queue.Enqueue(child);
                }
            }
            var remaining = children.ToDictionary(pair => pair.Key, pair => pair.Value.Count);
            foreach (var pair in remaining) if (pair.Value == 0) queue.Enqueue(pair.Key);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!parents.TryGetValue(node, out var owners)) continue;
                foreach (var owner in owners) if (--remaining[owner] == 0) queue.Enqueue(owner);
            }
            var direct = new HashSet<GraphNodeStorage>();
            foreach (var node in children.Keys)
                if (remaining[node] == 0 && node is GameItem item && !item.isDebugMenu &&
                    (!(item is GameItemElement element) || !element.IsDestroyed()) &&
                    GeneralHelper.IsItemOwned(item) && !GeneralHelper.IsInsideNonOwnedContainer(item) && match(item)) direct.Add(node);
            var aggregate = InspectionRules.Aggregate(roots, node => children[node], node => direct.Contains(node) ? ItemRisk.Stolen : ItemRisk.None);
            var visible = new HashSet<GameItemElement>();
            foreach (var pair in aggregate)
            {
                if (pair.Value == ItemRisk.None || remaining[pair.Key] != 0 || !(pair.Key is GameItemElement item) || item.IsDestroyed() ||
                    item.isDebugMenu || item.parentInventory == null || item.middle == null ||
                    !GeneralHelper.IsItemOwned(item) || GeneralHelper.IsInsideNonOwnedContainer(item) ||
                    !item.middle.gameObject.activeInHierarchy || item.handler?.FindParentWindow()?.IsVisible() != true) continue;
                visible.Add(item);
                if (!frames.TryGetValue(item, out var frame) || !frame.Alive)
                {
                    frame?.Dispose();
                    frames[item] = frame = new DemandFrame(item.middle.rectTransform);
                }
                frame.Geometry(item.modifiedShape);
                frame.Display(direct.Contains(item), children[item].Any(child => aggregate[child] != ItemRisk.None));
            }
            foreach (var item in frames.Keys.Where(item => !visible.Contains(item)).ToList()) { frames[item].Dispose(); frames.Remove(item); }
        }

        private sealed class DemandFrame : IDisposable
        {
            private readonly GameObject root;
            private readonly List<Image> edges = new List<Image>();
            private readonly Image badge;
            internal bool Alive => root != null;
            internal DemandFrame(RectTransform parent)
            {
                root = new GameObject("Better Hotkeys customer demand", typeof(RectTransform));
                var rect = (RectTransform)root.transform;
                rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = Vector2.zero;
                // Inset from the risk border so both independent modules remain visible.
                var inset = Settings.Current.Width + 2f;
                edges.Add(Image(new Vector2(0, 1), Vector2.one, new Vector2(inset, -inset - 2), new Vector2(-inset, -inset)));
                edges.Add(Image(Vector2.zero, new Vector2(1, 0), new Vector2(inset, inset), new Vector2(-inset, inset + 2)));
                edges.Add(Image(Vector2.zero, new Vector2(0, 1), new Vector2(inset, inset), new Vector2(inset + 2, -inset)));
                edges.Add(Image(new Vector2(1, 0), Vector2.one, new Vector2(-inset - 2, inset), new Vector2(-inset, -inset)));
                badge = Image(Vector2.one, Vector2.one, new Vector2(-inset - 8, -inset - 8), new Vector2(-inset, -inset));
            }
            private Image Image(Vector2 min, Vector2 max, Vector2 low, Vector2 high)
            {
                var obj = new GameObject("Demand marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var rect = (RectTransform)obj.transform; rect.SetParent(root.transform, false);
                rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high;
                var image = obj.GetComponent<Image>(); image.raycastTarget = false; return image;
            }
            internal void Geometry(GridShape shape) => ((RectTransform)root.transform).sizeDelta = new Vector2(shape.globalWidth, shape.globalHeight) * UISettings.current.gridSize;
            internal void Display(bool direct, bool nested)
            {
                var color = Settings.ColorFor(Settings.Current.DemandColor);
                foreach (var edge in edges) { edge.color = color; edge.gameObject.SetActive(direct); }
                badge.color = color; badge.gameObject.SetActive(nested);
            }
            public void Dispose() { if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); } }
        }
    }
}
