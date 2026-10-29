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
            StorageItemPicker.Reset();
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
            if (IsOpen && (editing == null || !StorageItemPicker.TargetValid(editing))) { editor?.Close(); editing = null; }
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
                        if (!ModEntry.Active || IsOpen || StorageItemPicker.IsPicking || ManualStoragePrompt.IsOpen || OrganizerWorld.Blocked()) return;
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
            var rule = StorageRulesStore.Get(target) ?? new StorageRule { All = false };
            OpenDraft(target, rule);
        }
        // Preview removals are explicit, immediate edits. Editor checkbox changes remain a draft.
        internal static void RemoveItem(StorageTarget target, string id, bool denied)
        {
            if (!ModEntry.Active || IsOpen || ManualStoragePrompt.IsOpen || StorageItemPicker.IsPicking || !StorageTargets.IsEligible(target)) return;
            var rule = StorageRulesStore.Get(target);
            if (rule == null) return;
            if (denied) rule.DeniedItems = rule.DeniedItems.Where(value => value != id).ToArray();
            else rule.AllowedItems = rule.AllowedItems.Where(value => value != id).ToArray();
            StorageRulesStore.Set(target, rule); nextRefresh = 0;
        }
        private static void OpenDraft(StorageTarget target, StorageRule rule)
        {
            editor?.Close(); editing = target;
            var run = PlayerStore.Instance;
            var chosen = new HashSet<string>(rule.Tags, StringComparer.Ordinal);
            var allowed = rule.AllowedItems.Distinct(StringComparer.Ordinal).ToList();
            var denied = rule.DeniedItems.Distinct(StringComparer.Ordinal).ToList();
            var options = StorageTagCatalog.Options();
            var b = CustomUIManager.Window(Settings.Id + ":storage", Texts.T("收纳规则 — 第 ", "Storage rules — page ") + (target.Page + 1), CustomUIManager.LAYER_FRONT)
                .SetSize(560, 720).Center().SetCloseOnEscape(true);
            editor = b.GetWindow();
            var window = editor;
            b.AddLabel(Texts.T("物品按同种记录；拒绝优先。修改仅在保存后生效。", "Items are matched by kind; denials win. Changes apply only after Save."));
            b.BeginScroll(480);
            b.AddInput(Texts.T("容器 / 分页名称", "Container / page name"), rule.Name, null, "name");
            b.AddDropdown(new[] { Texts.T("物品 + 标签", "Items + tags"), Texts.T("仅允许物品白名单", "Allowed items only") }, rule.ItemsOnly ? 1 : 0, null, "items_mode");
            b.AddDropdown(new[] { Texts.T("满足任意标签", "Match any tag"), Texts.T("满足全部标签", "Match all tags") }, rule.All ? 1 : 0, null, "match");
            b.AddLabel(Texts.T("优先级（整数，越大越优先）", "Priority (integer; higher first)"));
            b.AddInput("0", rule.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture), null, "priority");
            bool Capture(out StorageRule draft)
            {
                draft = new StorageRule();
                if (!int.TryParse(window.Get("priority").GetString(), out var priority))
                { window.Get("error").SetText(Texts.T("优先级必须是整数。", "Priority must be an integer.")); return false; }
                draft = new StorageRule { Name = window.Get("name").GetString(), Tags = chosen.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                    All = window.Get("match").GetIndex() == 1, Priority = priority, ItemsOnly = window.Get("items_mode").GetIndex() == 1,
                    AllowedItems = allowed.ToArray(), DeniedItems = denied.ToArray() };
                return true;
            }
            void Pick(bool reject)
            {
                if (!ModEntry.Active || !ReferenceEquals(run, PlayerStore.Instance) || !StorageItemPicker.TargetValid(target) || !Capture(out var draft)) return;
                window.Hide();
                var destination = (reject ? draft.DeniedItems : draft.AllowedItems).ToList();
                StorageItemPicker.Start(target, item => {
                    if (!destination.Contains(item.identifier)) { destination.Add(item.identifier); OrganizerInput.Notice(Texts.T("已加入草稿：", "Added to draft: ") + item.name); }
                }, resume => {
                    if (resume && ReferenceEquals(run, PlayerStore.Instance))
                    {
                        if (reject) draft.DeniedItems = destination.ToArray();
                        else draft.AllowedItems = destination.ToArray();
                        OpenDraft(target, draft);
                    }
                    else { window.Close(); if (ReferenceEquals(editor, window)) { editor = null; editing = null; } }
                });
            }
            b.AddButton(Texts.T("点选添加允许物品", "Pick allowed items"), () => Pick(false));
            b.AddButton(Texts.T("点选添加拒绝物品", "Pick denied items"), () => Pick(true));
            b.BeginScroll(110);
            b.AddLabel(Texts.T("已选物品（取消勾选移除，保存后生效）", "Selected items (uncheck to remove, then Save)"));
            foreach (var id in allowed.ToArray())
            {
                var value = id;
                b.AddToggle(Texts.T("允许：", "Allow: ") + StorageItemDisplay.Name(value), true, keep => { if (keep) { if (!allowed.Contains(value)) allowed.Add(value); } else allowed.Remove(value); });
            }
            foreach (var id in denied.ToArray())
            {
                var value = id;
                b.AddToggle(Texts.T("拒绝：", "Deny: ") + StorageItemDisplay.Name(value), true, keep => { if (keep) { if (!denied.Contains(value)) denied.Add(value); } else denied.Remove(value); });
            }
            b.End();
            var rows = new List<(string label, CustomUIElement element)>();
            b.AddInput(Texts.T("搜索游戏分类 / 属性", "Search game categories / attributes"), "", null, "search", query => {
                foreach (var row in rows) row.element.SetVisible(row.label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            });
            b.BeginScroll(180);
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
            b.End();
            b.AddLabel("", "error");
            b.AddButton(Texts.T("保存", "Save"), () => {
                if (!ModEntry.Active || !ReferenceEquals(run, PlayerStore.Instance) || !StorageItemPicker.TargetValid(target)) { window.Close(); return; }
                if (!Capture(out var draft)) return;
                try {
                    StorageRulesStore.Set(target, draft);
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
