using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterHotkeys
{
    internal static class Settings
    {
        internal const string Id = "probably_stolen_organizer";
        private static ModConfig? config;
        private static readonly Dictionary<string, ConfigEntry> entries = new Dictionary<string, ConfigEntry>();
        private static Preferences? pending;
        internal static Preferences Current { get; private set; } = new Preferences();
        internal static string Status { get; private set; } = "";
        internal static string Language => entries.TryGetValue("language", out var entry) ? ((ConfigEntry<string>)entry).Value : "Auto";
        internal static bool OwnsInput => ModEntry.Active && Current.Takeover;
        private static T Get<T>(string key) => ((ConfigEntry<T>)entries[key]).Value;
        private static void Bool(string key, bool value, string group, string zh, string en, string zhHelp = "", string enHelp = "") =>
            entries.Add(key, config!.Define(key, value, group, () => Texts.T(zh, en), () => Texts.T(zhHelp, enHelp), false));
        private static void Choice(string key, string value, string[] choices, string group, string zh, string en, string zhHelp = "", string enHelp = "") =>
            entries.Add(key, config!.Define(key, value, group, () => Texts.T(zh, en), choices, () => Texts.T(zhHelp, enHelp), false));
        private static void Float(string key, float value, float min, float max, string group, string zh, string en) =>
            entries.Add(key, config!.Define(key, value, group, () => Texts.T(zh, en), min, max, null, false));
        private static void Binding(string key, string value, string zh, string en) =>
            entries.Add(key, config!.Define(key, value, "02 Keys / 按键", () => Texts.T(zh, en), null,
                () => Texts.T("输入如 Shift、RightAlt、Ctrl+Q、B；None 清除绑定。F8/F9 保留。转移/固定需按住该键并左键单击。", "Examples: Shift, RightAlt, Ctrl+Q, B. None clears a binding. F8/F9 are reserved. Transfer/pin: hold the chord and left-click."), false));

        internal static void Init(ModManifest manifest)
        {
            config = ModConfig.For(manifest.ID);
            entries.Clear();
            Choice("language", "Auto", new[] { "Auto", "English", "简体中文" }, "01 General / 常规", "语言（重新打开面板刷新）", "Language (reopen panel to refresh)");
            Choice("input_mode", "TakeOver", new[] { "TakeOver", "VanillaWithHelpers" }, "01 General / 常规", "输入兼容模式", "Input compatibility mode",
                "TakeOver：助手接管，原生快捷转移停用。VanillaWithHelpers：原生转移，助手转移/固定关闭，保留整理与标记。", "TakeOver disables vanilla quick transfer. VanillaWithHelpers restores vanilla transfer and disables our transfer/pin gestures; sorting and markers remain available.");
            Bool("transfer_enabled", true, "01 General / 常规", "快速转移", "Quick transfer", "接管模式下关闭此项不会恢复原生转移；要恢复请选择 VanillaWithHelpers。", "Disabling this in TakeOver does not restore vanilla transfer. Select VanillaWithHelpers to restore it.");
            Bool("pin_enabled", true, "01 General / 常规", "快捷固定", "Quick pin");
            Bool("sort_enabled", true, "01 General / 常规", "标题栏整理", "Header sorting");
            Bool("markers_enabled", true, "01 General / 常规", "赃物/违禁品标记", "Stolen/contraband markers");
            Binding("counter_key", "Shift", "柜台转移键＋左键", "Sales-counter chord + left click");
            Binding("container_key", "Ctrl", "容器转移键＋左键", "Container chord + left click");
            Binding("pin_key", "Alt", "固定键＋左键", "Pin chord + left click");
            Binding("marker_key", "B", "标记键", "Marker chord");
            Binding("demand_key", "D", "顾客需求高亮开关", "Toggle customer demand");
            Binding("store_key", "C", "按规则收纳 / 按住并点击", "Store by rules / hold and click");
            Bool("demand_enabled", true, "06 Demand / 需求", "需求高亮（记忆开关）", "Demand highlight (remembered toggle)");
            Bool("auto_store", true, "07 Storage / 收纳", "按规则快速收纳", "Quick store by rules");
            Bool("empty_storage_fallback", false, "07 Storage / 收纳", "无可用规则目标时使用空规则容器", "Fall back to unconfigured containers");
            Bool("force_manual_storage", false, "07 Storage / 收纳", "手动指定目标时允许强制放入（不再提醒）", "Allow manual rule override without asking", "仅绕过本 Mod 收纳规则；自动收纳仍遵守规则。关闭此项恢复提醒。", "Only bypasses this mod's storage rules; automatic storage still obeys them. Turn off to restore prompts.");
            // Versioned key applies the new default once to existing installations; later user choices persist.
            Choice("store_scope_v2", "All", new[] { "All", "Open" }, "07 Storage / 收纳", "目标范围：All 全部页 / Open 当前可见页", "Targets: All pages / Open visible pages",
                "默认 All：包括关闭窗口的合规储藏页。Open：仅打开且可见的页。", "All (default): eligible pages including closed windows. Open: only open, visible pages.");
            Binding("sort_key", "S", "独立整理键", "Independent sort chord");
            Bool("equivalent_modifiers", true, "02 Keys / 按键", "左右修饰键等价", "Treat left/right modifiers equally", "关闭时 Shift/Ctrl/Alt 代表左键；右键请写 RightShift/RightCtrl/RightAlt。", "When off, Shift/Ctrl/Alt mean left-side keys. Use RightShift/RightCtrl/RightAlt for the right side.");
            Bool("follow_sort_key", true, "02 Keys / 按键", "整理跟随游戏键位", "Follow game's sort binding");
            Choice("marker_trigger", "DoublePress", new[] { "Press", "DoublePress", "Hold" }, "02 Keys / 按键", "标记触发：单按/双击/按住", "Markers: Press / DoublePress / Hold");
            Float("double_interval", .35f, .15f, .8f, "02 Keys / 按键", "双击间隔（秒）", "Double-press interval (seconds)");
            Bool("batch_selected", true, "03 Behavior / 行为", "操作同来源选区", "Apply to selection in the same inventory");
            Bool("target_header_only", false, "03 Behavior / 行为", "仅点击标题栏指定目标", "Select target only via its header");
            Bool("target_border", true, "03 Behavior / 行为", "目标容器高亮", "Highlight target container");
            Bool("remember_sort", true, "03 Behavior / 行为", "记住原生菜单的最近整理方式", "Remember last vanilla sort choice");
            Bool("sort_big_first", true, "03 Behavior / 行为", "大件优先", "Sort large items first");
            Bool("sort_from_end", false, "03 Behavior / 行为", "从末端排布", "Sort from the end");
            Bool("show_stolen", true, "04 Markers / 标记", "标记赃物", "Mark stolen items");
            Bool("show_contraband", true, "04 Markers / 标记", "标记违禁品", "Mark contraband");
            Bool("scan_nested", true, "04 Markers / 标记", "外层容器汇总嵌套内容", "Mark containers holding flagged items");
            Bool("owned_only", false, "04 Markers / 标记", "仅标记自有物品", "Only mark owned items");
            var colors = new[] { "Yellow", "Red", "Purple", "Cyan", "Orange", "White", "Green" };
            Choice("demand_color", "Cyan", colors, "06 Demand / 需求", "需求高亮颜色", "Demand color");
            Choice("stolen_color", "Yellow", colors, "04 Markers / 标记", "赃物颜色", "Stolen color");
            Choice("contraband_color", "Red", colors, "04 Markers / 标记", "违禁品颜色", "Contraband color");
            Choice("both_color", "Purple", colors, "04 Markers / 标记", "混合类别颜色", "Mixed-category color");
            entries.Add("border_width", config.Define("border_width", 2, "04 Markers / 标记", () => Texts.T("边框粗细", "Border width"), 1, 4, null, false));
            Float("border_opacity", 1f, .2f, 1f, "04 Markers / 标记", "边框不透明度", "Border opacity");
            Float("scan_interval", .2f, .1f, .5f, "04 Markers / 标记", "扫描间隔（秒）", "Scan interval (seconds)");
            Choice("transfer_notices", "Exceptions", new[] { "Exceptions", "All", "Off" }, "05 Notices / 提示", "转移提示：异常/全部/关闭", "Transfer notices: Exceptions / All / Off");
            Bool("target_notice", true, "05 Notices / 提示", "提示目标变化", "Announce target changes");
            Bool("marker_notice", true, "05 Notices / 提示", "提示标记开关", "Announce marker toggles");
            ModHook.OnModConfigApplied -= Applied;
            ModHook.OnModConfigApplied += Applied;
            saved.Clear();
            foreach (var pair in entries) saved[pair.Key] = pair.Value.BoxedDefault;
            Applied(Id);
            ApplyPending();
        }

        internal static void BeginApplyFeedback() =>
            Status = Texts.T("没有新的设置变更。", "No new setting changes.");

        private static void Applied(string id)
        {
            if (id != Id) return;
            var next = new Preferences {
                EmptyStorageFallback = Get<bool>("empty_storage_fallback"), ForceManualStorage = Get<bool>("force_manual_storage"),
                DemandEnabled = Get<bool>("demand_enabled"), AutoStore = Get<bool>("auto_store"), StoreAllPages = Get<string>("store_scope_v2") == "All", DemandColor = Get<string>("demand_color"),
                Takeover = Get<string>("input_mode") == "TakeOver", Transfer = Get<bool>("transfer_enabled"), Pin = Get<bool>("pin_enabled"), Sort = Get<bool>("sort_enabled"), Markers = Get<bool>("markers_enabled"),
                Equivalent = Get<bool>("equivalent_modifiers"), FollowSort = Get<bool>("follow_sort_key"), Batch = Get<bool>("batch_selected"), HeaderOnly = Get<bool>("target_header_only"), TargetBorder = Get<bool>("target_border"),
                RememberSort = Get<bool>("remember_sort"), BigFirst = Get<bool>("sort_big_first"), FromEnd = Get<bool>("sort_from_end"),
                Stolen = Get<bool>("show_stolen"), Contraband = Get<bool>("show_contraband"), Nested = Get<bool>("scan_nested"), OwnedOnly = Get<bool>("owned_only"),
                Trigger = Get<string>("marker_trigger"), DoubleInterval = Get<float>("double_interval"), Width = Get<int>("border_width"), Opacity = Get<float>("border_opacity"), Refresh = Get<float>("scan_interval"),
                StolenColor = Get<string>("stolen_color"), ContrabandColor = Get<string>("contraband_color"), BothColor = Get<string>("both_color"),
                Notices = Get<string>("transfer_notices"), TargetNotice = Get<bool>("target_notice"), MarkerNotice = Get<bool>("marker_notice")
            };
            var keys = new[] { "counter_key", "container_key", "pin_key", "marker_key", "sort_key", "demand_key", "store_key" };
            var old = new[] { Current.Counter, Current.Container, Current.PinKey, Current.MarkerKey, Current.SortKey, Current.DemandKey, Current.StoreKey };
            var parsed = new Shortcut[7];
            var valid = true;
            for (var i = 0; i < keys.Length; i++) valid &= Shortcut.TryParse(Get<string>(keys[i]), out parsed[i]);
            var active = new[] { next.Takeover && next.Transfer, next.Takeover && next.Transfer, next.Takeover && next.Pin, next.Markers, next.Sort && !next.FollowSort, true, next.AutoStore };
            for (var i = 0; i < keys.Length && valid; i++)
                for (var j = 0; j < i && valid; j++)
                    if (active[i] && active[j] && parsed[i].Overlaps(parsed[j], next.Equivalent)) valid = false;
            if (!valid)
            {
                for (var i = 0; i < keys.Length; i++) ((ConfigEntry<string>)entries[keys[i]]).Set(old[i].Text);
                // Keep the whole previously validated runtime snapshot; no partially applied mode/alias changes.
                RestorePreferences();
                Status = Texts.T("按键无效或重复；已恢复上一套有效配置。重新打开面板查看。", "Invalid or overlapping bindings. Previous valid settings restored; reopen this panel.");
                ModEntry.Log?.Warning(Status);
                pending = null;
                return;
            }
            next.Counter = parsed[0]; next.Container = parsed[1]; next.PinKey = parsed[2]; next.MarkerKey = parsed[3]; next.SortKey = parsed[4]; next.DemandKey = parsed[5]; next.StoreKey = parsed[6];
            pending = next;
            Status = Texts.T("已保存；当前操作结束后生效。语言修改后请重新打开面板。", "Saved; applies after the current gesture. Reopen this panel after changing language.");
        }

        private static readonly Dictionary<string, object> saved = new Dictionary<string, object>();
        private static void RestorePreferences()
        {
            foreach (var pair in saved)
            {
                var entry = entries[pair.Key];
                if (entry is ConfigEntry<bool> b) b.Set((bool)pair.Value);
                else if (entry is ConfigEntry<int> i) i.Set((int)pair.Value);
                else if (entry is ConfigEntry<float> f) f.Set((float)pair.Value);
                else if (entry is ConfigEntry<string> s) s.Set((string)pair.Value);
            }
        }
        internal static void ApplyPending()
        {
            if (pending == null) return;
            Current = pending; pending = null;
            saved.Clear();
            foreach (var entry in entries) saved[entry.Key] = entry.Value.BoxedValue;
            OrganizerInput.ConfigurationChanged();
            InspectionOverlay.ConfigurationChanged();
            CustomerDemandOverlay.ConfigurationChanged();
        }
        internal static void RememberManualOverride()
        {
            Current.ForceManualStorage = true;
            if (pending != null) pending.ForceManualStorage = true;
            ((ConfigEntry<bool>)entries["force_manual_storage"]).Set(true);
            saved["force_manual_storage"] = true;
        }
        internal static void ToggleDemand()
        {
            Current.DemandEnabled = !Current.DemandEnabled;
            ((ConfigEntry<bool>)entries["demand_enabled"]).Set(Current.DemandEnabled);
            saved["demand_enabled"] = Current.DemandEnabled;
            if (pending != null) pending.DemandEnabled = Current.DemandEnabled;
            CustomerDemandOverlay.ConfigurationChanged();
        }
        internal static void RememberSort(bool big, bool end)
        {
            if (!Current.RememberSort) return;
            Current.BigFirst = big; Current.FromEnd = end;
            ((ConfigEntry<bool>)entries["sort_big_first"]).Set(big);
            ((ConfigEntry<bool>)entries["sort_from_end"]).Set(end);
            saved["sort_big_first"] = big; saved["sort_from_end"] = end;
        }
        internal static Color ColorFor(string name)
        {
            var color = name == "Yellow" ? new Color(1f,.85f,.15f) : name == "Red" ? new Color(1f,.2f,.2f) :
                name == "Purple" ? new Color(.85f,.3f,1f) : name == "Cyan" ? Color.cyan :
                name == "Orange" ? new Color(1f,.55f,.1f) : name == "Green" ? Color.green : Color.white;
            color.a = Current.Opacity; return color;
        }
    }
}
