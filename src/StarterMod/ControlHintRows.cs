using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterMod
{
    internal sealed class ControlHintRow
    {
        internal string Key { get; }
        internal string Action { get; }
        internal ControlHintRow(string key, string action) { Key = key; Action = action; }
    }

    internal static class ControlHintRows
    {
        internal static List<ControlHintRow> Build(bool active, Preferences p, bool chinese,
            Func<string, string> nativeKey, Func<Shortcut, bool, string> conflict)
        {
            var rows = new List<ControlHintRow>();
            if (!active) return rows;
            string T(string zh, string en) => chinese ? zh : en;
            void Add(string key, string action) => rows.Add(new ControlHintRow(key, action));
            void Bound(Shortcut key, bool enabled, string gesture, string action, bool sorting = false)
            {
                var label = string.Join("+", key.Keys.Select(k => Shortcut.Canonical(k, p.Equivalent)));
                if (!enabled) label = T("已关闭", "Disabled");
                else if (key.IsNone) label = T("未绑定", "Unbound");
                else
                {
                    label = gesture.Replace("{key}", label);
                    if (conflict(key, sorting).Length > 0) label += T(" [冲突]", " [Conflict]");
                }
                Add(label, action);
            }
            var click = T("{key} + 左键", "{key} + Click");
            if (p.Takeover)
            {
                Bound(p.Counter, p.Transfer, click, T("库存/容器 → 出售柜台；柜台 → 库存", "Inventory/container → sales counter; counter → inventory"));
                Bound(p.Container, p.Transfer, click, T("库存 → 所选自有容器；容器 → 库存", "Inventory → selected owned container; container → inventory"));
                Bound(p.PinKey, p.Pin, click, T("固定 / 解除固定", "Pin / unpin"));
            }
            else
            {
                Add(nativeKey("quick_move_inventory") + T(" + 左键", " + Click"), T("原生：快速转移至库存", "Vanilla: quick transfer to inventory"));
                Add(nativeKey("quick_move_showcase") + T(" + 左键", " + Click"), T("原生：快速转移至展示柜", "Vanilla: quick transfer to display case"));
                Add(nativeKey("quick_move_storage") + T(" + 左键", " + Click"), T("原生：快速转移至柜台", "Vanilla: quick transfer to counter"));
            }
            var sortAction = T("悬停容器标题栏：整理该容器", "Hover container header: sort that inventory");
            if (p.FollowSort) Add(p.Sort ? nativeKey("sort_menu") : T("已关闭", "Disabled"), sortAction);
            else Bound(p.SortKey, p.Sort, "{key}", sortAction, true);
            var markerGesture = p.Trigger == "Hold" ? T("按住 {key}", "Hold {key}") :
                p.Trigger == "DoublePress" ? T("双击 {key}", "Double {key}") : T("单按 {key}", "Press {key}");
            Bound(p.MarkerKey, p.Markers, markerGesture, T("赃物 / 违禁品标记", "Stolen / contraband markers"));
            Add(nativeKey("rotate_ccw"), T("拖动物品时：逆时针旋转", "While dragging: rotate counterclockwise"));
            Add(nativeKey("rotate_cw"), T("拖动物品时：顺时针旋转", "While dragging: rotate clockwise"));
            Add(nativeKey("flip_item"), T("拖动物品时：翻转", "While dragging: flip item"));
            Add("Esc", T("关闭界面 / 打开主菜单", "Close UI / open main menu"));
            Add(nativeKey("advance_dialogue"), T("跳过对话打字效果", "Skip dialogue typewriter"));
            Bound(p.DemandKey, true, "{key}", T("顾客需求高亮开关（记忆状态）", "Toggle customer demand (remembered)"));
            Bound(p.StoreKey, p.AutoStore, T("{key} / 按住＋左键", "{key} / hold + click"), T("选区按规则收纳 / 连续点击收纳", "Store selection by rules / click items to store"));
            return rows;
        }
    }
}
