using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using TMPro;

namespace StarterMod
{
    internal static class StorageEditor
    {
        private sealed class Header
        {
            internal PixelWindow Window = null!;
            internal MachineryButton Button = null!;
            internal string Original = "", Applied = "", Requested = "";
            internal TextOverflowModes Overflow;
        }
        private static readonly Dictionary<PixelWindow, Header> headers = new Dictionary<PixelWindow, Header>();
        private static readonly System.Reflection.FieldInfo TitleText = GameAccess.Field(typeof(PixelWindow), "titleText");
        private static readonly System.Reflection.FieldInfo CloseButton = GameAccess.Field(typeof(PixelWindow), "topCloseButton");
        private static CustomUIWindow? editor;
        private static StorageTarget? editing;
        private static float nextRefresh;
        internal static bool IsOpen => editor != null && editor.IsAlive && editor.IsVisible;
        internal static void Reset()
        {
            editor?.Close(); editor = null; editing = null;
            var oldHeaders = headers.Values.ToList(); headers.Clear();
            foreach (var header in oldHeaders) Remove(header); nextRefresh = 0;
        }
        private static void Remove(Header h)
        {
            if (h.Window.handler == null) return;
            var textNode = TitleText.GetValue(h.Window) as TreeNodeRender;
            if (textNode != null) textNode.text.overflowMode = h.Overflow;
            if (h.Window.titleString == h.Applied) h.Window.SetTitle(h.Original);
            h.Window.RemoveHeaderButton(h.Button);
            if (h.Button.handler != null) UnityEngine.Object.Destroy(h.Button.handler.gameObject);
        }
        internal static void Tick()
        {
            if (CustomUIManager.Instance == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .2f;
            if (IsOpen && (editing == null || !StorageTargets.IsEligible(editing))) { editor?.Close(); editing = null; }
            var targets = StorageTargets.Enumerate(true);
            var visible = new HashSet<PixelWindow>(targets.Select(t => t.Window));
            foreach (var old in headers.Keys.Where(w => !visible.Contains(w)).ToList()) { var removed = headers[old]; headers.Remove(old); Remove(removed); }
            foreach (var target in targets)
            {
                var w = target.Window;
                if (!headers.TryGetValue(w, out var h))
                {
                    h = new Header { Window = w, Original = w.titleString, Overflow = ((TreeNodeRender)TitleText.GetValue(w)).text.overflowMode };
                    h.Button = new MachineryButton(CustomUIManager.Instance.spriteSetting, () => {
                        if (!ModEntry.Active || IsOpen || OrganizerWorld.Blocked()) return;
                        var current = StorageTargets.FromWindow(w);
                        if (current != null) Open(current);
                    }, 16);
                    w.AddHeaderButton(h.Button); headers.Add(w, h);
                }
                if (w.titleString != h.Applied && w.titleString != h.Original) h.Original = w.titleString;
                
                var rule = StorageRulesStore.Get(target);
                var name = string.IsNullOrEmpty(rule?.Name) ? h.Original : rule!.Name;
                var summary = rule == null || rule.Tags.Length == 0 ? "" : " [" + string.Join(" / ", rule.Tags.Take(2).Select(StorageTagCatalog.Label)) + (rule.Tags.Length > 2 ? " …" : "") + "]";
                var requested = name + summary;
                if (h.Requested != requested || w.titleString != h.Applied)
                { w.SetTitle(requested); h.Requested = requested; h.Applied = w.titleString; }
                Layout(w);
                h.Button.SetDescription(Texts.T("编辑当前容器 / 当前分页的名称与收纳规则", "Edit this container / current page's name and storage rules"));
            }
        }
        internal static void Layout(PixelWindow window)
        {
            if ((int)window.style == 1 || !headers.TryGetValue(window, out var h)) return;
            var close = (TreeNodeRender)CloseButton.GetValue(window);
            var rect = h.Button.handler.rectTransform;
            rect.anchoredPosition = new Vector2(close.rectTransform.anchoredPosition.x - 19, close.rectTransform.anchoredPosition.y);
            rect.SetAsLastSibling();
            var title = (TreeNodeRender)TitleText.GetValue(window);
            title.text.overflowMode = TextOverflowModes.Ellipsis;
            title.rectTransform.sizeDelta = new Vector2(Math.Max(0, rect.anchoredPosition.x - title.rectTransform.anchoredPosition.x - 3), title.rectTransform.sizeDelta.y);
        }
        private static void Open(StorageTarget target)
        {
            editor?.Close(); editing = target;
            var rule = StorageRulesStore.Get(target) ?? new StorageRule { All = false };
            var chosen = new HashSet<string>(rule.Tags, StringComparer.Ordinal);
            var options = StorageTagCatalog.Options();
            var b = CustomUIManager.Window(Settings.Id + ":storage", Texts.T("收纳规则 — 第 ", "Storage rules — page ") + (target.Page + 1), CustomUIManager.LAYER_FRONT)
                .SetSize(560, 650).Center().SetCloseOnEscape(true);
            editor = b.GetWindow();
            b.AddLabel(Texts.T("名称自由填写；空标签不会自动收纳，手动放入不受影响。", "Choose any name. Empty tags disable automatic storage only."));
            b.AddInput(Texts.T("容器 / 分页名称", "Container / page name"), rule.Name, null, "name");
            b.AddDropdown(new[] { Texts.T("满足任意标签", "Match any tag"), Texts.T("满足全部标签", "Match all tags") }, rule.All ? 1 : 0, null, "match");
            b.AddLabel(Texts.T("优先级（整数，越大越优先）", "Priority (integer; higher first)"));
            b.AddInput("0", rule.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture), null, "priority");
            var rows = new List<(string label, CustomUIElement element)>();
            b.AddInput(Texts.T("搜索游戏分类 / 属性", "Search game categories / attributes"), "", null, "search", query => {
                foreach (var row in rows) row.element.SetVisible(row.label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            });
            b.BeginScroll(300);
            foreach (var option in options)
            {
                var id = option.Id;
                b.AddToggle(option.Label, chosen.Contains(id), value => { if (value) chosen.Add(id); else chosen.Remove(id); });
                rows.Add((option.Label, b.Last));
            }
            // Preserve previously saved rules even if their attribute is not currently observable.
            foreach (var id in chosen.Except(options.Select(o => o.Id)).ToArray())
            {
                b.AddToggle(StorageTagCatalog.Label(id), true, value => { if (value) chosen.Add(id); else chosen.Remove(id); });
            }
            b.End();
            b.AddLabel("", "error");
            var window = editor;
            b.AddButton(Texts.T("保存", "Save"), () => {
                if (!ModEntry.Active || !StorageTargets.IsEligible(target)) { window.Close(); return; }
                if (!int.TryParse(window.Get("priority").GetString(), out var priority))
                { window.Get("error").SetText(Texts.T("优先级必须是整数。", "Priority must be an integer.")); return; }
                try {
                    StorageRulesStore.Set(target, new StorageRule { Name = window.Get("name").GetString(), Tags = chosen.OrderBy(x => x, StringComparer.Ordinal).ToArray(), All = window.Get("match").GetIndex() == 1, Priority = priority });
                    window.Close(); nextRefresh = 0;
                } catch (Exception e) { window.Get("error").SetText(Texts.T("保存失败：", "Save failed: ") + e.Message); }
            });
            b.AddButton(Texts.T("取消", "Cancel"), () => window.Close());
            b.Show();
        }
    }
}

namespace StarterMod
{
    [HarmonyPatch(typeof(PixelWindow), "Validate")]
    internal static class StorageHeaderLayoutPatch
    {
        private static void Postfix(PixelWindow __instance)
        { if (ModEntry.Active) StorageEditor.Layout(__instance); }
    }
}
