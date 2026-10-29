using System;
using System.Collections.Generic;
using System.Linq;
namespace BetterHotkeys
{
    internal static class ManualStoragePrompt
    {
        private static CustomUIWindow? window;
        internal static bool IsOpen => window != null && window.IsAlive && window.IsVisible;
        internal static void Reset() { window?.Close(); window = null; }
        internal static bool Accepts(StorageTarget target, GameItem item) =>
            StorageItemRules.ManualAllowed(StorageRulesStore.Get(target), item.identifier, tag => StorageTagCatalog.Matches(item, tag));
        internal static void Confirm(StorageTarget target, List<GameItemElement> items, Action<bool> commit)
        {
            Reset();
            var conflicts = items.Where(i => !i.IsDestroyed() && !Accepts(target, i)).ToList();
            if (conflicts.Count == 0 || Settings.Current.ForceManualStorage) { commit(Settings.Current.ForceManualStorage); return; }
            // The modal pauses native input updates, including tooltip exit handling.
            // Reset its cached owner before showing the confirmation, not after transfer.
            ItemTooltipHandler.current?.UpdateReset();
            OverlayHandler.current?.HideTooltip();
            var b = CustomUIManager.Window(Settings.Id + ":manual-storage", Texts.T("目标收纳规则不接受部分物品", "Some items do not match the destination rules"), CustomUIManager.LAYER_FRONT)
                .SetSize(520, 440).Center().SetCloseOnEscape(true);
            window = b.GetWindow();
            b.AddLabel(Texts.T("强制仅绕过本 Mod 的收纳规则，仍遵守游戏空间、锁定及类型限制。", "Override bypasses only this mod's rules; game space, locks and type restrictions still apply."));
            b.BeginScroll(200);
            foreach (var group in conflicts.GroupBy(i => i.identifier))
                b.AddLabel(group.First().GetDisplayName(true) + " × " + group.Sum(i => (long)i.unitCount));
            b.End();
            var remember = false;
            b.AddToggle(Texts.T("以后不再提醒，允许手动强制放入", "Don't ask again; allow manual overrides"), false, value => remember = value);
            void Complete(bool force)
            {
                Reset();
                if (!ModEntry.Active || !OrganizerWorld.InStore || !StorageTargets.IsEligible(target) ||
                    StorageTargets.FromWindow(target.Window)?.Grid != target.Grid)
                { OrganizerInput.Notice(Texts.T("目标容器或分页已变化，已取消本次转移。", "Destination or page changed; transfer cancelled.")); return; }
                if (force && remember) Settings.RememberManualOverride();
                try { commit(force); GameAccess.RefreshSelected(); }
                catch (Exception e) { ModEntry.Fail(e); }
            }
            b.AddButton(Texts.T("仅转移符合项", "Move matching items only"), () => Complete(false));
            b.AddButton(Texts.T("全部强制放入", "Override for all items"), () => Complete(true));
            b.AddButton(Texts.T("取消", "Cancel"), Reset);
            b.Show();
        }
    }
}
