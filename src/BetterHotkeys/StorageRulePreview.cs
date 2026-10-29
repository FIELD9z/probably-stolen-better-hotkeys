using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterHotkeys
{
    // A rendering-only sibling of window.inner: never attached to the inventory storage graph.
    internal static class StorageRulePreview
    {
        internal static Action<StorageTarget, string, bool>? RemoveItem { get; set; } = StorageEditor.RemoveItem;
        private static readonly Dictionary<PixelWindow, Panel> panels = new Dictionary<PixelWindow, Panel>();
        private static readonly FieldInfo[] Backgrounds = new[] { "background", "outsideBorder", "machineryPlate", "machineryIntake" }
            .Select(name => GameAccess.Field(typeof(PixelWindow), name)).ToArray();
        private static readonly FieldInfo ResizeButton = GameAccess.Field(typeof(PixelWindow), "bottomResizeButton");
        private static float next;
        private static bool failed;

        internal static void Reset()
        {
            foreach (var panel in panels.Values) panel.Dispose();
            panels.Clear(); next = 0; failed = false; StorageItemDisplay.Reset();
        }
        internal static void Tick()
        {
            if (!ModEntry.Active || !OrganizerWorld.InStore) { Reset(); return; }
            if (failed || Time.unscaledTime < next) return;
            next = Time.unscaledTime + .2f;
            try
            {
                var targets = StorageTargets.Enumerate(true);
                var visible = new HashSet<PixelWindow>(targets.Select(target => target.Window));
                foreach (var window in panels.Keys.Where(window => !visible.Contains(window)).ToList())
                { panels[window].Dispose(); panels.Remove(window); }
                foreach (var target in targets)
                {
                    var rule = StorageRulesStore.Get(target);
                    if (!panels.TryGetValue(target.Window, out var panel))
                        panels[target.Window] = panel = new Panel(target);
                    panel.Refresh(target, rule ?? new StorageRule());
                }
            }
            catch (Exception error)
            {
                Reset(); failed = true;
                ModEntry.Log?.Warning("Storage preview restored after UI error: " + error.Message);
            }
        }
        internal static void BeforeValidate(PixelWindow window)
        { if (panels.TryGetValue(window, out var panel)) panel.UndoLayout(); }
        internal static void AfterValidate(PixelWindow window)
        {
            if (!ModEntry.Active || !panels.TryGetValue(window, out var panel)) return;
            try { panel.ApplyLayout(); }
            catch (Exception error) { Reset(); failed = true; ModEntry.Log?.Warning("Storage preview layout restored: " + error.Message); }
        }

        private sealed class Panel : IDisposable
        {
            private readonly PixelWindow window;
            private readonly GameObject root;
            private readonly TMP_FontAsset font;
            private readonly List<(RectTransform rect, Vector2 size, Vector2 position)> changed = new List<(RectTransform, Vector2, Vector2)>();
            private StorageTarget target;
            private StorageRule rule = null!;
            private string signature = "";
            private bool expanded;
            private float height;
            private bool laidOut;
            internal Panel(StorageTarget target)
            {
                this.target = target; window = target.Window;
                var template = window.handler.GetComponentInChildren<TextMeshProUGUI>(true);
                if (template == null) throw new InvalidOperationException("No native window text font found.");
                font = template.font;
                root = new GameObject("Better Hotkeys storage rule preview", typeof(RectTransform));
                root.transform.SetParent(window.handler.transform, false);
            }
            internal void Refresh(StorageTarget current, StorageRule value)
            {
                target = current; rule = value;
                var key = current.Page + ":" + StorageRuleCodec.Write(value) + ":" + window.widthPixels + ":" + Settings.Language + ":" + LocHelper.GetCurrentLocaleCode();
                if (signature != key) { signature = key; Build(); }
                ApplyLayout();
            }
            internal void UndoLayout()
            {
                if (!laidOut) return;
                foreach (var entry in changed)
                    if (entry.rect != null) { entry.rect.sizeDelta = entry.size; entry.rect.anchoredPosition = entry.position; }
                changed.Clear(); laidOut = false;
            }
            private void Remember(RectTransform rect)
            { if (!changed.Any(entry => entry.rect == rect)) changed.Add((rect, rect.sizeDelta, rect.anchoredPosition)); }
            internal void ApplyLayout()
            {
                if (root == null || window.inner == null) return;
                if (laidOut) { ClampVisibleBounds(); return; }
                var inner = window.inner.rectTransform;
                var top = -inner.anchoredPosition.y;
                var rect = (RectTransform)root.transform;
                Place(rect, inner.anchoredPosition.x, top, Math.Max(48, window.widthPixels - window.xPadding * 2), height);
                Remember(inner); inner.anchoredPosition += new Vector2(0, -height);
                foreach (var field in Backgrounds)
                    if (field.GetValue(window) is TreeNodeRender node && node != null)
                    { Remember(node.rectTransform); node.rectTransform.sizeDelta += new Vector2(0, height); }
                if (ResizeButton.GetValue(window) is TreeNodeRender resize && resize != null)
                { Remember(resize.rectTransform); resize.rectTransform.anchoredPosition += new Vector2(0, -height); }
                laidOut = true;
                ClampVisibleBounds();
            }
            private void ClampVisibleBounds()
            {
                if (!window.canMove || window.handler == null) return;
                var handler = window.handler.rectTransform;
                var parent = handler.parent;
                if (parent == null) return;
                // Native movable windows may live on LetterboxManager.DockRoot. Nested window
                // renderers, however, belong to their owner's layout and must not be translated here.
                if (parent.GetComponentInParent<TreeNodeRender>() != null) return;
                var canvas = handler.GetComponentInParent<Canvas>();
                var viewport = canvas == null ? null : canvas.rootCanvas.transform as RectTransform;
                if (viewport == null || viewport.rect.height <= 0) return;
                var minimum = float.PositiveInfinity;
                var maximum = float.NegativeInfinity;
                var corners = new Vector3[4];
                void Include(RectTransform bounds)
                {
                    bounds.GetWorldCorners(corners);
                    foreach (var corner in corners)
                    {
                        var y = viewport.InverseTransformPoint(corner).y;
                        minimum = Math.Min(minimum, y); maximum = Math.Max(maximum, y);
                    }
                }
                Include((RectTransform)root.transform);
                foreach (var field in Backgrounds)
                    if (field.GetValue(window) is TreeNodeRender node && node != null && node.gameObject.activeInHierarchy)
                        Include(node.rectTransform);
                var title = GameAccess.TitleNode(window);
                if (title != null) Include(title.rectTransform);
                var low = viewport.rect.yMin + 2;
                var high = viewport.rect.yMax - 2;
                float delta;
                if (maximum - minimum > high - low)
                    delta = high - maximum; // Oversized: keep its title/top visible, never alternate edges.
                else if (minimum < low) delta = low - minimum;
                else if (maximum > high) delta = high - maximum;
                else return;
                if (Math.Abs(delta) < .25f) return;
                var worldDelta = viewport.TransformVector(new Vector3(0, delta, 0));
                var parentDelta = parent.InverseTransformVector(worldDelta);
                handler.anchoredPosition += new Vector2(parentDelta.x, parentDelta.y);
            }
            private void Build()
            {
                UndoLayout();
                foreach (Transform child in root.transform) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); }
                var width = Math.Max(48, window.widthPixels - window.xPadding * 2);
                var diagnostic = StorageItemRules.Diagnostic(rule);
                if (!StorageItemRules.HasLogic(rule))
                {
                    height = 14;
                    var emptyBg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                    emptyBg.color = new Color(.08f, .09f, .11f, .97f); emptyBg.raycastTarget = true;
                    Text(root.transform, Texts.T("未配置", "Not configured"), 2, 0, width - 4, 14, Color.gray);
                    return;
                }
                height = 68 + (expanded ? 36 : 0) + (diagnostic.Length == 0 ? 0 : 16);
                var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
                bg.color = new Color(.08f, .09f, .11f, .97f); bg.raycastTarget = true;
                Button(root.transform, expanded ? "v" : ">", width - 18, 0, 18, 14, () => { expanded = !expanded; Build(); ApplyLayout(); });
                Text(root.transform, Texts.T("允许", "Allow"), 2, 0, width - 22, 12, Color.white);
                Row(rule.AllowedItems, false, 12, width);
                Text(root.transform, Texts.T("拒绝", "Deny"), 2, 34, width - 4, 12, new Color(.85f, .85f, .85f));
                Row(rule.DeniedItems, true, 46, width);
                var y = 68f;
                if (expanded)
                {
                    var text = rule.ItemsOnly ? Texts.T("仅白名单物品", "Whitelist items only") :
                        (rule.All ? Texts.T("标签：全部满足", "Tags: all match") : Texts.T("标签：任一满足", "Tags: any match")) + "\n" + string.Join(" / ", rule.Tags.Select(StorageTagCatalog.Label));
                    var summary = Text(root.transform, text, 2, y, width - 4, 34, Color.white);
                    summary.raycastTarget = true;
                    CustomUIElement.Bind(summary.gameObject, "Storage rule summary").SetTooltip(text);
                    y += 36;
                }
                if (diagnostic.Length != 0)
                {
                    var warning = diagnostic == "EmptyWhitelist" ? Texts.T("! 白名单为空", "! Whitelist is empty") :
                        diagnostic == "DeniedWhitelist" ? Texts.T("! 白名单均被拒绝", "! All allowed items are denied") :
                        Texts.T("! 没有可接收规则", "! No receiving rule");
                    Text(root.transform, warning, 2, y, width - 4, 16, new Color(1, .75f, .25f));
                }
            }
            private void Row(string[] ids, bool denied, float y, float width)
            {
                var viewport = Rect("Rule strip", root.transform, 0, y, width, 22);
                var background = viewport.gameObject.AddComponent<Image>(); background.color = new Color(.14f, .15f, .17f); background.raycastTarget = true;
                viewport.gameObject.AddComponent<RectMask2D>();
                var content = Rect("Items", viewport, 0, 0, Math.Max(width, ids.Length * 28), 22);
                var scroll = viewport.gameObject.AddComponent<ScrollRect>();
                scroll.viewport = viewport; scroll.content = content; scroll.horizontal = true; scroll.vertical = false;
                scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 20;
                if (ids.Length == 0) Text(content, Texts.T("无", "None"), 3, 0, width - 6, 22, Color.gray);
                for (var i = 0; i < ids.Length; i++)
                {
                    var id = ids[i]; var x = i * 28f;
                    var tile = Rect("Rule " + id, content, x, 0, 26, 22);
                    var edge = tile.gameObject.AddComponent<Image>(); edge.color = denied ? Color.black : Color.white;
                    var inside = Rect("Preview sprite", tile, 1, 1, 24, 20);
                    var image = inside.gameObject.AddComponent<Image>(); image.raycastTarget = false;
                    var sprite = StorageItemDisplay.Sprite(id);
                    if (sprite != null) { image.sprite = sprite; image.preserveAspect = true; image.color = new Color(1, 1, 1, .45f); }
                    else { image.color = new Color(.3f, .3f, .3f); Text(inside, "?", 0, 0, 24, 20, Color.white); }
                    Text(tile, denied ? "−" : "+", 1, 9, 10, 12, denied ? new Color(1, .4f, .4f) : Color.white);
                    var remove = tile.gameObject.AddComponent<Button>(); remove.targetGraphic = edge;
                    CustomUIElement.Bind(tile.gameObject, "Storage rule item").SetTooltip(StorageItemDisplay.Name(id) + "\n" +
                        Texts.T("点击从此规则移除", "Click to remove from this rule") + (sprite == null ? "\n" + Texts.T("图标资源不可用", "Icon resource unavailable") : ""));
                    remove.onClick.AddListener(() => {
                        if (!ModEntry.Active || StorageEditor.IsOpen || StorageItemPicker.IsPicking || ManualStoragePrompt.IsOpen || !StorageTargets.IsEligible(target)) return;
                        RemoveItem?.Invoke(target, id, denied); signature = "";
                    });
                }
            }
            private TextMeshProUGUI Text(Transform parent, string value, float x, float y, float width, float h, Color color)
            {
                var rect = Rect("Label", parent, x, y, width, h);
                var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
                var loader = FontLoader.Instance;
                text.font = loader == null ? font : Texts.Chinese ? loader.simplifiedChineseFont : loader.notoSans;
                text.fontSize = 8;
                text.text = value; text.color = color; text.raycastTarget = false; text.enableWordWrapping = true;
                text.overflowMode = TextOverflowModes.Ellipsis; return text;
            }
            private void Button(Transform parent, string value, float x, float y, float width, float h, Action click)
            {
                var rect = Rect("Expand rule", parent, x, y, width, h); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.25f,.25f,.25f);
                var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(() => {
                    if (!ModEntry.Active || StorageEditor.IsOpen || StorageItemPicker.IsPicking || ManualStoragePrompt.IsOpen) return;
                    click();
                });
                Text(rect, value, 3, 0, width - 3, h, Color.white);
            }
            public void Dispose() { UndoLayout(); if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); } }
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        { var go = new GameObject(name, typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent, false); Place(rect,x,y,w,h); return rect; }
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        { rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1); rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(width,height); }
    }

    [HarmonyPatch(typeof(PixelWindow), "Validate")]
    internal static class StoragePreviewLayoutPatch
    {
        private static void Prefix(PixelWindow __instance) => StorageRulePreview.BeforeValidate(__instance);
        private static void Postfix(PixelWindow __instance) => StorageRulePreview.AfterValidate(__instance);
    }
}
