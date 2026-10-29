// Small deterministic doubles for testing the transfer boundary; these do not emulate Unity or validate game loading.
namespace BetterHotkeys;

internal static class Texts { internal static string T(string zh, string en) => en; }

internal class GameInventory { }
internal class GameGridInventory : GameInventory
{
    internal bool Locked;
    internal bool Allowed = true;
    internal int FreeCapacity;
    internal GameGridInventory? Redirect;
    internal GridShape inventoryShape = new GridShape();
    internal List<GameItem> childItems = new();
    internal bool IsInsertLocked() => Locked;
    internal bool MayHaveValidInventorySlot(GameItem _) => Allowed;
    internal SlotMarker? TryInventorySlot(GameItem item, int count, GridShapeBuilder shape, object state) =>
        FreeCapacity <= 0 ? null : new SlotMarker {
            item = item, inventory = Redirect ?? this, numTransfer = Math.Min(count, FreeCapacity)
        };
}
internal class GameItem
{
    internal int unitCount = 20;
    internal int RemoveLimit = int.MaxValue;
    internal int AddCapacity;
    internal bool CanRemove = true;
    internal bool Pinned;
    internal bool Stackable;
    internal bool Cycle;
    internal bool RejectCommit;
    internal GameInventory parentInventory = new();
    internal GridShape modifiedShape = new();
    internal object modifiedState = new();
    internal bool MayRemove() => CanRemove;
    internal int MaxNumRemove() => CanRemove ? Math.Min(RemoveLimit, unitCount) : 0;
    internal int MaxNumAdd() => AddCapacity;
    internal bool MayStack(GameItem other) => Stackable && other.Stackable;
}
internal class GameItemElement : GameItem
{
    internal bool IsDestroyed() => unitCount == 0;
}
internal class GridShape { internal int width = 1; internal int height = 1; }
internal class GridShapeBuilder
{
    internal GridShapeBuilder(GridShape _) { }
    internal void SetTransform(int x, int y, bool flipped, int rotation) { }
}
internal class SlotMarker
{
    internal GameItem item = null!;
    internal GameGridInventory inventory = null!;
    internal GameItem? targetItem;
    internal int numTransfer;
    internal bool IsValid() => numTransfer > 0;
    internal int TryAcceptOnce(int request)
    {
        if (item.RejectCommit) return 0;
        var amount = Math.Min(request, numTransfer);
        if (targetItem != null)
        {
            targetItem.AddCapacity -= amount;
            targetItem.unitCount += amount;
            item.unitCount -= amount;
        }
        else
        {
            inventory.FreeCapacity -= amount;
            if (amount == item.unitCount) item.parentInventory = inventory;
            else item.unitCount -= amount;
        }
        return amount;
    }
}
internal static class GeneralHelper { internal static bool MayPlayerInsertInto(GameGridInventory target) => target.Allowed; }
internal static class ItemPinHelper { internal static bool IsPinned(GameItem item) => item.Pinned; }
internal static class OrganizerWorld { internal static bool WouldContainSelf(GameItem item, GameInventory _) => item.Cycle; }
