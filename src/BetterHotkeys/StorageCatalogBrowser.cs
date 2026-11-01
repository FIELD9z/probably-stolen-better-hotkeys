using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterHotkeys
{
    // Two independent selected lists stay above the searchable, collapsible candidates.
    // Every control edits the caller's draft; only the editor's Save commits rules.
    internal sealed class StorageCatalogBrowser
    {
        internal readonly StorageCatalogModel Model;
        internal Action? OnBrowseAll;
        private readonly CustomUIWindow window;
        private readonly RectTransform[] selected = new RectTransform[2];
        private readonly CustomUIElement[] selectedTitles = new CustomUIElement[2];
        private readonly CustomUIElement[] clearButtons = new CustomUIElement[2];
        private readonly Dictionary<(string, bool), List<CustomUIElement>> toggles = new Dictionary<(string, bool), List<CustomUIElement>>();
        private readonly RectTransform candidates;
        private readonly CustomUIElement search, count;

        internal StorageCatalogBrowser(CustomUIBuilder b, StorageCatalogModel model, float candidateHeight)
        {
            Model = model; window = b.GetWindow();
            b.AddLabel(Texts.T("已选规则（不受下方筛选影响；保存后生效）", "Selected rules (independent of filters; apply after Save)"));
            b.BeginRow();
            for (var i = 0; i < 2; i++)
            {
                var index = i;
                b.BeginColumn();
                CustomUIElement.Bind(b.CurrentContainer.gameObject, "").SetFlexible(1, 0).SetPreferredSize(1, 0);
                b.CurrentContainer.GetComponent<LayoutElement>().minWidth = 0;
                b.BeginRow();
                b.AddLabel(""); selectedTitles[i] = b.Last; selectedTitles[i].SetFlexible(1, 0);
                b.AddButton(Texts.T("清空本组", "Clear group"), () => {
                    (index == 1 ? Model.Denied : Model.Allowed).Clear(); RefreshSelection();
                });
                clearButtons[i] = b.Last; clearButtons[i].SetFixedWidth(98);
                b.End();
                b.BeginScroll(96); selected[i] = b.CurrentContainer; b.End();
                b.End();
            }
            b.End();
            b.AddInput(Texts.T("搜索名称 / 拼音首字母 / 全拼", "Search name / pinyin initials / full pinyin"), "", null, null, query => {
                Model.Search(query); OnBrowseAll?.Invoke(); RenderCandidates(true);
            });
            search = b.Last;
            b.BeginRow();
            b.AddLabel(""); count = b.Last; count.SetFlexible(1, 0);
            b.AddButton(Texts.T("展开全部", "Expand all"), () => {
                foreach (var group in Model.Options.SelectMany(o => o.Groups)) Model.Expanded.Add(group);
                RenderCandidates(false);
            });
            b.Last.SetFixedWidth(110);
            b.AddButton(Texts.T("折叠全部", "Collapse all"), () => { Model.Expanded.Clear(); RenderCandidates(false); });
            b.Last.SetFixedWidth(110);
            b.End();
            b.BeginScroll(candidateHeight); candidates = b.CurrentContainer; b.End();
            RefreshSelection(); RenderCandidates(true);
        }

        internal void Inspect(IEnumerable<string> ids)
        { Model.Inspect(ids); search.SetString("", false); RenderCandidates(true); }
        internal void ShowAll()
        { Model.Search(""); search.SetString("", false); OnBrowseAll?.Invoke(); RenderCandidates(true); }
        internal void Collect(bool denied) { Model.Collect(denied); RefreshSelection(); }

        private CustomUIBuilder Builder(RectTransform root)
        {
            CustomUIBuilder.ClearContainer(root);
            return new CustomUIBuilder(CustomUIManager.Instance, window, root);
        }
        private static void NameLabel(CustomUIBuilder b, string label)
        {
            b.AddLabel(label);
            b.Last.SetFlexible(1, 0).SetPreferredSize(1, 28).SetTooltip(label);
            b.Last.root.GetComponent<LayoutElement>().minWidth = 0;
            if (b.Last.text != null) { b.Last.text.enableWordWrapping = false; b.Last.text.overflowMode = TextOverflowModes.Ellipsis; }
        }
        private void RefreshSelection()
        {
            for (var i = 0; i < 2; i++)
            {
                var reject = i == 1;
                var entries = Model.Selected(reject);
                selectedTitles[i].SetText((reject ? Texts.T("排除", "Exclude") : Texts.T("允许", "Allow")) + " (" + entries.Length + ")");
                clearButtons[i].SetInteractable(entries.Length != 0);
                var b = Builder(selected[i]);
                if (entries.Length == 0) b.AddLabel(Texts.T("暂无", "None"));
                foreach (var option in entries)
                {
                    b.BeginRow(); NameLabel(b, option.Label);
                    b.AddButton(Texts.T("移除", "Remove"), () => { Model.Set(option.Id, reject, false); RefreshSelection(); });
                    b.Last.SetFixedWidth(72); b.End();
                }
            }
            foreach (var pair in toggles)
                foreach (var control in pair.Value)
                    control.SetBool((pair.Key.Item2 ? Model.Denied : Model.Allowed).Contains(pair.Key.Item1), false);
            CustomUIBuilder.RebuildLayout(window.ContentRoot);
        }
        private void RenderCandidates(bool resetScroll)
        {
            toggles.Clear();
            var b = Builder(candidates);
            var entries = Model.Candidates();
            count.SetText(Texts.T("候选：", "Candidates: ") + entries.Length);
            if (entries.Length == 0) b.AddLabel(Texts.T("没有匹配项。已选规则仍保留在上方。", "No matches. Selected rules remain above."));
            else if (Model.Sample != null || CatalogSearch.Normalize(Model.Query).Length != 0)
                foreach (var option in entries) Candidate(b, option); // Search is global and deduplicated.
            else
                foreach (var group in entries.SelectMany(o => o.Groups).Distinct().OrderBy(g => g, StringComparer.CurrentCulture))
                {
                    var members = entries.Where(o => o.Groups.Contains(group)).ToArray();
                    var expanded = Model.Expanded.Contains(group);
                    b.AddButton((expanded ? "− " : "+ ") + group + " (" + members.Length + ")", () => {
                        if (!Model.Expanded.Remove(group)) Model.Expanded.Add(group);
                        RenderCandidates(false);
                    });
                    b.Last.SetFlexible(1, 0);
                    if (expanded) foreach (var option in members) Candidate(b, option);
                }
            CustomUIBuilder.RebuildLayout(window.ContentRoot);
            if (resetScroll)
            {
                var scroll = candidates.GetComponentInParent<ScrollRect>();
                if (scroll != null) scroll.verticalNormalizedPosition = 1;
            }
        }
        private void Candidate(CustomUIBuilder b, CatalogOption option)
        {
            b.BeginRow(); NameLabel(b, option.Label);
            for (var i = 0; i < 2; i++)
            {
                var reject = i == 1;
                b.AddToggle(reject ? Texts.T("排除", "Exclude") : Texts.T("允许", "Allow"),
                    (reject ? Model.Denied : Model.Allowed).Contains(option.Id), value => {
                        Model.Set(option.Id, reject, value); RefreshSelection();
                    });
                b.Last.SetFixedWidth(100);
                var key = (option.Id, reject);
                if (!toggles.TryGetValue(key, out var controls)) toggles[key] = controls = new List<CustomUIElement>();
                controls.Add(b.Last);
            }
            b.End();
        }
    }
}
