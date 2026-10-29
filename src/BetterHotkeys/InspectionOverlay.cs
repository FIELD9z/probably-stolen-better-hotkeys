using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BetterHotkeys
{
    internal static class InspectionOverlay
    {
        private static readonly DoubleTap tap = new DoubleTap();
        private static readonly Dictionary<GameItemElement, RiskFrame> frames = new Dictionary<GameItemElement, RiskFrame>();
        private static bool enabled;
        private static float nextRefresh;
        internal static void ConfigurationChanged()
        {
            tap.Reset();
            ClearFrames();
            nextRefresh = 0;
            if (!Settings.Current.Markers || Settings.Current.Trigger == "Hold") enabled = false;
        }

        internal static void Reset()
        {
            enabled = false;
            tap.Reset();
            ClearFrames();
        }

        private static void ClearFrames()
        {
            foreach (var frame in frames.Values) frame.Dispose();
            frames.Clear();
        }

        internal static void Tick(bool blocked)
        {
            var prefs = Settings.Current;
            if (!prefs.Markers) { if (enabled) Reset(); return; }
            var keyDown = RuntimeBindings.Down(prefs.MarkerKey);
            var eligible = !blocked &&
                (PaperEditor.Instance == null || !PaperEditor.Instance.IsEditing) &&
                (DialogUIManager.Instance == null || !DialogUIManager.Instance.IsChoice()) &&
                !UIDragHandler.isDraggingAnyUI &&
                (ItemMouseDragHandler.current == null || !ItemMouseDragHandler.current.IsDraggingItem) &&
                (ItemMultiSelectHandler.current == null || !ItemMultiSelectHandler.current.IsGroupDragging);
            if (eligible && (keyDown || (prefs.Trigger == "Hold" && RuntimeBindings.Held(prefs.MarkerKey))) && !RuntimeBindings.Allowed(prefs.MarkerKey, false, keyDown))
            {
                eligible = false;
            }
            var toggle = prefs.Trigger == "DoublePress" ? tap.Update(Time.unscaledTime, keyDown, eligible, prefs.DoubleInterval) : prefs.Trigger == "Press" && keyDown && eligible;
            var held = prefs.Trigger == "Hold" && eligible && RuntimeBindings.Held(prefs.MarkerKey);
            if (toggle || (prefs.Trigger == "Hold" && held != enabled))
            {
                enabled = prefs.Trigger == "Hold" ? held : !enabled;
                nextRefresh = 0;
                if (!enabled) ClearFrames();
                if (prefs.MarkerNotice && prefs.Trigger != "Hold") OrganizerInput.Notice(enabled ?
                    EnabledNotice(prefs) : Texts.T("物品标记已关闭。", "Item markers off."));
            }
            if (!enabled) return;
            foreach (var pair in frames)
                if (!pair.Key.IsDestroyed() && pair.Value.IsAlive) pair.Value.UpdateGeometry(pair.Key.modifiedShape);
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + prefs.Refresh;
            Refresh();
        }

        private static string EnabledNotice(Preferences prefs)
        {
            if (!prefs.Stolen && !prefs.Contraband)
                return Texts.T("标记已开启，但赃物和违禁品类别均已关闭，不显示边框。", "Markers are on, but both categories are disabled; no borders will be shown.");
            var categories = new List<string>();
            if (prefs.Stolen) categories.Add(Texts.T("赃物：", "Stolen: ") + Texts.ColorName(prefs.StolenColor));
            if (prefs.Contraband) categories.Add(Texts.T("违禁品：", "Contraband: ") + Texts.ColorName(prefs.ContrabandColor));
            if (prefs.Stolen && prefs.Contraband) categories.Add(Texts.T("混合：", "Mixed: ") + Texts.ColorName(prefs.BothColor));
            return Texts.T("标记开启：", "Markers on: ") + string.Join(" / ", categories);
        }

        private static ItemRisk DirectRisk(GraphNodeStorage node)
        {
            if (!(node is GameItem item) || item.isDebugMenu || (item is GameItemElement element && element.IsDestroyed()))
                return ItemRisk.None;
            // Read tags only: inspection/security routines can change heat or run random detection.
            if (Settings.Current.OwnedOnly && (!GeneralHelper.IsItemOwned(item) || GeneralHelper.IsInsideNonOwnedContainer(item))) return ItemRisk.None;
            var result = Settings.Current.Stolen && item.IsTag("STOLEN_TAG") ? ItemRisk.Stolen : ItemRisk.None;
            if (Settings.Current.Contraband && ContrabandHelper.IsContraband(item)) result |= ItemRisk.Contraband;
            return result;
        }

        private static void Refresh()
        {
            var windows = WindowsHandler.current?.visibleWindows;
            if (windows == null) { ClearFrames(); return; }
            var roots = windows.Where(window => window != null && window.IsVisible()).Cast<GraphNodeStorage>().ToList();
            var risks = InspectionRules.Aggregate(roots, node => node.children, DirectRisk);
            var visible = new HashSet<GameItemElement>();
            foreach (var pair in risks)
            {
                var risk = Settings.Current.Nested ? pair.Value : DirectRisk(pair.Key);
                if (risk == ItemRisk.None || !(pair.Key is GameItemElement item) || item.IsDestroyed() ||
                    item.isDebugMenu || item.parentInventory == null || item.middle == null ||
                    !item.middle.gameObject.activeInHierarchy || item.handler == null ||
                    item.handler.FindParentWindow()?.IsVisible() != true) continue;
                visible.Add(item);
                if (!frames.TryGetValue(item, out var frame) || !frame.IsAlive)
                {
                    frame?.Dispose();
                    frames[item] = frame = new RiskFrame(item.middle.rectTransform);
                }
                frame.UpdateGeometry(item.modifiedShape);
                frame.SetRisk(risk);
            }
            foreach (var item in frames.Keys.Where(item => !visible.Contains(item)).ToList())
            {
                frames[item].Dispose();
                frames.Remove(item);
            }
        }

        private sealed class RiskFrame : IDisposable
        {
            private readonly GameObject root;
            private readonly List<Image> edges = new List<Image>();
            internal bool IsAlive => root != null;
            internal RiskFrame(RectTransform parent)
            {
                // middle tracks the transformed top-left, but retains unrotated width/height.
                root = new GameObject("Organizer risk marker", typeof(RectTransform));
                var rect = (RectTransform)root.transform;
                rect.SetParent(parent, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = Vector2.zero;
                var width = Settings.Current.Width;
                Edge(new Vector2(0, 1), Vector2.one, new Vector2(0, -width), Vector2.zero);
                Edge(Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, width));
                Edge(Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(width, 0));
                Edge(new Vector2(1, 0), Vector2.one, new Vector2(-width, 0), Vector2.zero);
            }
            internal void UpdateGeometry(GridShape shape)
            {
                var cell = UISettings.current.gridSize;
                ((RectTransform)root.transform).sizeDelta = new Vector2(shape.globalWidth * cell, shape.globalHeight * cell);
            }
            private void Edge(Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
            {
                var edge = new GameObject("Risk edge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var rect = (RectTransform)edge.transform;
                rect.SetParent(root.transform, false);
                rect.anchorMin = min; rect.anchorMax = max;
                rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
                var image = edge.GetComponent<Image>();
                image.raycastTarget = false;
                edges.Add(image);
            }
            internal void SetRisk(ItemRisk risk)
            {
                var color = Settings.ColorFor(risk == ItemRisk.Stolen ? Settings.Current.StolenColor : risk == ItemRisk.Contraband ? Settings.Current.ContrabandColor : Settings.Current.BothColor);
                foreach (var edge in edges) edge.color = color;
            }
            public void Dispose()
            {
                if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            }
        }
    }
}
