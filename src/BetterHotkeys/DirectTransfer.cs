using System;
using System.Linq;

namespace BetterHotkeys
{
    internal static class DirectTransfer
    {
        // Return moved units, unlike GraphUtils.TryAcceptOnce, whose return value is the remainder.
        internal static int Move(GameItemElement item, GameGridInventory target, out string reason)
        {
            reason = "";
            var source = item.parentInventory;
            if (source == target) { reason = Texts.T("来源与目标相同", "Source and destination are the same"); return 0; }
            if (ItemPinHelper.IsPinned(item)) { reason = Texts.T("物品已固定", "Item is pinned"); return 0; }
            if (OrganizerWorld.WouldContainSelf(item, target)) { reason = Texts.T("不能放入自身或内部容器", "Cannot move a container into itself or its descendants"); return 0; }
            if (!item.MayRemove() || item.MaxNumRemove() < 1 || target.IsInsertLocked() ||
                !GeneralHelper.MayPlayerInsertInto(target) || !target.MayHaveValidInventorySlot(item))
            { reason = Texts.T("原生规则不允许转移", "Native rules do not allow this transfer"); return 0; }

            var requested = item.unitCount;
            var moved = 0;
            while (moved < requested && !item.IsDestroyed() && item.parentInventory == source)
            {
                var slot = FindSlot(item, target, requested - moved);
                if (slot == null) { reason = Texts.T("目标空间不足或没有合法位置", "Destination is full or has no valid placement"); break; }
                // Keep callbacks from silently redirecting a nominally direct slot into another inventory.
                if (slot.inventory != target || (slot.targetItem != null && slot.targetItem.parentInventory != target))
                { reason = Texts.T("原生回调改变了目标，已停止", "Native callback changed the destination; stopped"); break; }
                var accepted = slot.TryAcceptOnce(requested - moved); // SlotMarker returns moved units.
                if (accepted <= 0) { reason = Texts.T("目标未接收物品", "Destination did not accept the item"); break; }
                moved += accepted;
            }
            return moved;
        }

        private static SlotMarker? FindSlot(GameItem item, GameGridInventory target, int requested)
        {
            var removable = Math.Min(requested, item.MaxNumRemove());
            if (removable < 1) return null;
            // Same direct-layer stacking rules as vanilla; no item-slot passthrough search.
            foreach (var other in target.childItems)
            {
                if (other == item || !item.MayStack(other)) continue;
                var count = Math.Min(removable, other.MaxNumAdd());
                if (count > 0)
                    return new SlotMarker { item = item, inventory = target, targetItem = other, numTransfer = count };
            }

            // Vanilla tries four rotations and both flips. Only the destination is searched.
            var shape = new GridShapeBuilder(item.modifiedShape);
            for (var flip = 0; flip < 2; flip++)
                for (var rotation = 0; rotation < 4; rotation++)
                    for (var y = 0; y < target.inventoryShape.height; y++)
                        for (var x = 0; x < target.inventoryShape.width; x++)
                        {
                            shape.SetTransform(x, y, flip != 0, 4 - rotation);
                            var slot = target.TryInventorySlot(item, removable, shape, item.modifiedState);
                            if (slot != null && slot.IsValid() && slot.inventory == target && slot.targetItem == null)
                                return slot;
                        }
            return null;
        }
    }
}
