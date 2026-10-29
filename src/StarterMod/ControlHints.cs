using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace StarterMod
{
    // Only the existing help overlay's Shortcut region is replaced. No localization table or key binding is changed.
    internal static class ControlHints
    {
        private static Transform? owner;
        private static GameObject? root;
        private static TextMeshProUGUI? heading;
        private static readonly List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
        private static readonly List<(GameObject node, bool active)> originals = new List<(GameObject, bool)>();
        private static float nextRefresh;
        private static GameObject? failedOverlay;

        internal static void Restore()
        {
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            root = null; owner = null; heading = null; labels.Clear();
            foreach (var original in originals)
                if (original.node != null) original.node.SetActive(original.active);
            originals.Clear();
            nextRefresh = 0;
        }

        internal static void Tick(StoreUIManager manager)
        {
            if (!ModEntry.Active) { Restore(); return; }
            var overlay = manager.controlHintOverlay;
            if (overlay == null || !overlay.activeInHierarchy || overlay == failedOverlay) return;
            try
            {
                var region = overlay.transform.Find("Shortcut");
                if (region == null) return;
                if (owner != region || root == null)
                {
                    Restore();
                    Create(region);
                }
                if (Time.unscaledTime < nextRefresh) return;
                nextRefresh = Time.unscaledTime + .15f;
                Refresh();
            }
            catch (Exception error)
            {
                Restore();
                failedOverlay = overlay;
                ModEntry.Log?.Warning("Control hints restored to vanilla after a UI error: " + error.Message);
            }
        }

        private static void Create(Transform region)
        {
            // Validate the known full-game layout before hiding anything; leave unknown versions untouched.
            if (region.Find("TransferInv") == null || region.Find("TransferDisplay") == null ||
                region.Find("TransferCounter") == null || region.Find("VerticalLayout") == null)
                throw new InvalidOperationException("Unrecognized native shortcut panel.");
            var template = region.GetComponentInChildren<TextMeshProUGUI>(true);
            if (template == null) throw new InvalidOperationException("No native hint font template.");
            foreach (Transform child in region) originals.Add((child.gameObject, child.gameObject.activeSelf));
            owner = region;
            root = new GameObject("Better Hotkeys hints", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            rect.SetParent(region, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(14, 8); rect.offsetMax = new Vector2(-14, -8);
            heading = Label(rect, template, 0, 1, 0, 27);
            for (var i = 0; i < 12; i++)
            {
                labels.Add(Label(rect, template, 0, .36f, 27 + i * 25, 24));
                labels.Add(Label(rect, template, .37f, 1, 27 + i * 25, 24));
            }
            Refresh(); // Only hide native nodes once a populated replacement exists.
            foreach (var original in originals) original.node.SetActive(false);
        }

        private static TextMeshProUGUI Label(RectTransform parent, TextMeshProUGUI template,
            float left, float right, float top, float height)
        {
            var obj = new GameObject("Hint text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var text = obj.GetComponent<TextMeshProUGUI>();
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(left, 1); rect.anchorMax = new Vector2(right, 1);
            rect.offsetMin = new Vector2(0, -top - height); rect.offsetMax = new Vector2(0, -top);
            text.font = template.font; text.color = template.color;
            text.fontSize = 14; text.enableAutoSizing = true; text.fontSizeMin = 10; text.fontSizeMax = 14;
            text.enableWordWrapping = true; text.richText = false;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }

        private static void Refresh()
        {
            var p = Settings.Current;
            var rows = ControlHintRows.Build(ModEntry.Active, p, Texts.Chinese,
                id => KeyBindHelper.KeyName(KeyBindHelper.Get(id)), (key, sort) => RuntimeBindings.Conflict(key, sort));
            var fontLoader = FontLoader.Instance;
            var font = fontLoader == null ? null : Texts.Chinese ? fontLoader.simplifiedChineseFont : fontLoader.notoSans;
            heading!.text = Texts.Name + (p.Takeover ? Texts.T(" — 店内快捷键（接管）", " — in-store shortcuts (TakeOver)") : Texts.T(" — 原生转移 + 辅助", " — vanilla transfer + helpers"));
            if (font != null) heading.font = font;
            for (var i = 0; i < rows.Count; i++)
            {
                labels[i * 2].text = rows[i].Key;
                labels[i * 2 + 1].text = rows[i].Action;
            }
            if (font != null) foreach (var label in labels) label.font = font;
        }
    }

    [HarmonyPatch(typeof(StoreUIManager), "Update")]
    internal static class ControlHintRefreshPatch
    {
        private static void Postfix(StoreUIManager __instance) => ControlHints.Tick(__instance);
    }
}
