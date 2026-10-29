using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarterMod
{
    internal static class OrganizerWorld
    {
        internal static bool InStore => PlayerStore.Instance != null && EmporiumEntry.Instance != null &&
            PlayerStore.Instance.playerLocation == PlayerStore.PlayerLocation.IN_STORE &&
            MapUIManager.Instance != null && !MapUIManager.Instance.isOutside;

        internal static bool Blocked()
        {
            var ui = StoreUIManager.Instance;
            if (!InStore || ui == null || StoreUIManager.IsAnyTyping() || KeyBindHelper.SuppressHotkeys) return true;
            if (PaperEditor.Instance != null && PaperEditor.Instance.IsEditing) return true;
            ui.CheckRaycast(); // Do not depend on StoreUIManager.Update running before this frame's input.
            return ui.hoveringStoreUI || (ui.gameOptionUIManager != null && ui.gameOptionUIManager.IsOpen) ||
                (ui.escapeUIManager != null && ui.escapeUIManager.menuPanel.activeInHierarchy) ||
                ItemPinHelper.BlocksClicks || MachineStorageCoordinator.IsBusy || HopperHelper.IsBusy ||
                GameAccess.IsOverlayBlocking() || (ItemSelectHandler.current != null && ItemSelectHandler.current.IsEquipped);
        }

        internal static TreeNodeRender? PointerNode()
        {
            if (EventSystem.current == null) return null;
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, hits);
            // A non-game UI hit in front of an inventory must not be clicked through.
            return hits.Count == 0 ? null : hits[0].gameObject.GetComponentInParent<TreeNodeRender>();
        }

        internal static GameGridInventory? DirectGrid(PixelWindow? window)
        {
            if (window == null || !window.IsVisible() || window.childElement is MachineryContainer) return null;
            // Expanded storage wraps all pages in a panel row. Only the selected page is a target.
            if (window.childElement is MachineryPanelRow panels)
            {
                var tabs = panels.GetPanel("storage")?.content as MachineryTabs;
                if (tabs == null || tabs.selected < 0 || tabs.selected >= tabs.count) return null;
                var page = tabs.GetPage(tabs.selected);
                return (page is MachineryIntake intake ? intake.content : page) as GameGridInventory;
            }
            var found = new List<GameGridInventory>();
            var visited = new HashSet<GraphNodeStorage>();
            void Visit(GraphNodeStorage? node)
            {
                if (node == null || !visited.Add(node) || node is GameItem || node is MachineryContainer) return;
                if (node is GameInventory inventory)
                {
                    if (inventory is GameGridInventory grid) found.Add(grid);
                    return; // Never search containers stored in this inventory.
                }
                foreach (var child in node.children) Visit(child);
            }
            Visit(window.child);
            return found.Count == 1 ? found[0] : null;
        }

        internal static bool IsContainer(PixelWindow? window, out GameGridInventory? grid)
        {
            grid = DirectGrid(window);
            if (grid == null || window == null || window.isLocked || window.parentItems.Count == 0) return false;
            // A content window belongs to its physical container, not to a machine or an examine panel.
            return window.parentItems.All(owner => owner != null && owner.contentWindow == window &&
                !(owner is GameItemElement element && element.IsDestroyed()) &&
                GeneralHelper.IsItemOwned(owner) && !GeneralHelper.IsInsideNonOwnedContainer(owner)) &&
                GeneralHelper.MayPlayerInsertInto(grid);
        }

        internal static Place Classify(GameInventory? inventory)
        {
            if (!InStore || inventory == null) return Place.Unsupported;
            if (inventory == EmporiumEntry.Instance.invElement) return Place.Inventory;
            if (inventory == EmporiumEntry.Instance.backInvinvElement) return Place.Counter;
            if (WindowsHandler.current == null) return Place.Unsupported;
            foreach (var window in WindowsHandler.current.visibleWindows)
                if (IsContainer(window, out var grid) && grid == inventory) return Place.Container;
            return Place.Unsupported;
        }

        internal static bool InScope(GameItemElement? item) => item != null && !item.IsDestroyed() &&
            !item.isDebugMenu && GeneralHelper.IsItemOwned(item) &&
            !GeneralHelper.IsInsideNonOwnedContainer(item) && Classify(item.parentInventory) != Place.Unsupported;

        internal static bool IsHeader(TreeNodeRender? node, PixelWindow window)
        {
            var title = GameAccess.TitleNode(window);
            if (title == null || node == null) return false;
            // Title text and background have distinct nodes; the rectangle plus top-hit window disambiguates them.
            var canvas = title.GetComponentInParent<Canvas>();
            var camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (node.FindParentWindow() != window ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(title.rectTransform, Input.mousePosition, camera, out var point)) return false;
            var bounds = title.rectTransform.rect;
            bounds.width = Math.Max(0, window.widthPixels - 2 * window.xPadding);
            return bounds.Contains(point);
        }

        internal static bool WouldContainSelf(GameItem item, GameInventory target)
        {
            var pending = new Stack<GraphNodeStorage>();
            var seen = new HashSet<GraphNodeStorage>();
            pending.Push(target);
            while (pending.Count > 0)
            {
                var node = pending.Pop();
                if (ReferenceEquals(node, item)) return true;
                if (!seen.Add(node)) continue;
                foreach (var parent in node.parents) if (parent != null) pending.Push(parent);
            }
            return false;
        }
    }

    internal sealed class TargetHighlight : IDisposable
    {
        private GameObject? root;
        private PixelWindow? owner;
        internal void Show(PixelWindow window)
        {
            Dispose();
            if (!Settings.Current.TargetBorder) return;
            var title = GameAccess.TitleNode(window);
            if (title == null) return;
            owner = window;
            root = new GameObject("Organizer target", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.SetParent(title.rectTransform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0, 1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Refresh();
            // Additive non-raycasting border: do not mutate vanilla title color or focus state.
            Edge("top", new Vector2(0, 1), Vector2.one, new Vector2(0, -2), Vector2.zero);
            Edge("bottom", Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 2));
            Edge("left", Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(2, 0));
            Edge("right", new Vector2(1, 0), Vector2.one, new Vector2(-2, 0), Vector2.zero);
        }
        internal void Refresh()
        {
            if (root == null || owner == null) return;
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(Math.Max(0, owner.widthPixels - 2 * owner.xPadding), 0);
            rect.anchoredPosition = new Vector2(rect.sizeDelta.x * rect.pivot.x, 0);
        }
        private void Edge(string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)edge.transform;
            rect.SetParent(root!.transform, false);
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var image = edge.GetComponent<Image>();
            image.color = new Color(0.25f, 1f, 0.55f, 0.95f);
            image.raycastTarget = false;
        }
        public void Dispose()
        {
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            root = null;
            owner = null;
        }
    }
}
