using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarterMod
{
    internal static class AutoStorage
    {
        private static bool consumingClick;
        internal static void Reset() { consumingClick = false; }
        // A click consumed on press must remain consumed through release, even if the chord is released first.
        internal static bool BeforeInput(bool blocked)
        {
            if (consumingClick)
            {
                if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0)) consumingClick = false;
                return false;
            }
            if (blocked || !Settings.Current.AutoStore || UIDragHandler.isDraggingAnyUI ||
                (ItemMouseDragHandler.current?.IsDraggingItem ?? false) ||
                (ItemMultiSelectHandler.current?.IsGroupDragging ?? false)) return true;
            var key = Settings.Current.StoreKey;
            var pressed = RuntimeBindings.Down(key);
            var clicked = RuntimeBindings.Held(key) && Input.GetMouseButtonDown(0);
            if ((!pressed && !clicked) || !RuntimeBindings.Allowed(key)) return true;
            var items = pressed ? GameAccess.Selected() : new List<GameItemElement>();
            if (clicked)
            {
                var item = OrganizerWorld.PointerNode()?.FindParentElement() as GameItemElement;
                if (item != null && !items.Contains(item)) items.Add(item);
                // Do not swallow buttons, page tabs or window dragging when no inventory item was clicked.
                if (item != null) consumingClick = true;
            }
            if (items.Count != 0) Store(items);
            return !consumingClick;
        }
        private static void Store(List<GameItemElement> items)
        {
            var targets = StorageTargets.Enumerate(!Settings.Current.StoreAllPages)
                .Select(t => (target: t, rule: StorageRulesStore.Get(t)))
                .Where(t => t.rule != null && t.rule.Tags.Length != 0)
                .OrderByDescending(t => t.rule!.Priority).ThenBy(t => t.target.Owner.uniqueId).ThenBy(t => t.target.Page).ToList();
            var moved = 0; var remaining = 0;
            foreach (var item in items.Distinct().OrderBy(i => i.modifiedShape.minY).ThenBy(i => i.modifiedShape.minX).ThenBy(i => i.uniqueId).ToList())
            {
                if (item.IsDestroyed()) continue;
                var count = item.unitCount;
                if (!OrganizerWorld.InScope(item) || ItemPinHelper.IsPinned(item) || StorageRulesStore.HasTags(item)) { remaining += count; continue; }
                var sent = 0;
                foreach (var entry in targets)
                {
                    if (item.IsDestroyed() || sent >= count) break;
                    var rule = entry.rule!;
                    if (!StorageMatchRules.Matches(rule.Tags, rule.All, tag => StorageTagCatalog.Matches(item, tag))) continue;
                    var target = entry.target;
                    if (item.parentInventory == target.Grid) { sent = count; break; } // Already correctly stored.
                    if (!StorageTargets.IsEligible(target)) continue;
                    var accepted = DirectTransfer.Move(item, target.Grid, out _);
                    sent += accepted; moved += accepted;
                }
                remaining += Math.Max(0, count - sent);
            }
            GameAccess.RefreshSelected();
            if (remaining > 0 || Settings.Current.Notices == "All")
                OrganizerInput.Notice(Texts.T($"按标签收纳：已转移 {moved} 个，未收纳 {remaining} 个（未匹配、空间不足、固定或受规则限制）。", $"Stored by tags: moved {moved}, left {remaining} (no match, full, pinned or restricted)."));
        }
    }
}
