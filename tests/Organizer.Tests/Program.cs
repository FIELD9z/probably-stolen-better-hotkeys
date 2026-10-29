using BetterHotkeys;

var passed = 0;
void Check(string name, bool condition)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
    Console.WriteLine("PASS: " + name);
}

var routes = new[] {
    (Place.Inventory, Modifier.Shift, Place.Counter),
    (Place.Counter, Modifier.Shift, Place.Inventory),
    (Place.Container, Modifier.Shift, Place.Counter),
    (Place.Inventory, Modifier.Ctrl, Place.Container),
    (Place.Container, Modifier.Ctrl, Place.Inventory),
    (Place.Counter, Modifier.Ctrl, Place.Unsupported)
};
foreach (var (source, modifier, expected) in routes)
    Check($"route {source} / {modifier}", OrganizerRules.Destination(source, modifier) == expected);
foreach (var source in Enum.GetValues<Place>())
    foreach (var modifier in new[] { Modifier.None, Modifier.Alt, Modifier.Ctrl | Modifier.Shift, Modifier.Alt | Modifier.Shift })
        Check($"no transfer {source} / {modifier}", OrganizerRules.Destination(source, modifier) == Place.Unsupported);
Check("unsupported source never transfers", OrganizerRules.Destination(Place.Unsupported, Modifier.Shift) == Place.Unsupported);

Check("click is deferred until release", OrganizerRules.Evaluate(Modifier.Shift, Modifier.Shift, false, false, false, true) == GestureResult.Pending);
Check("shift click transfers", OrganizerRules.Evaluate(Modifier.Shift, Modifier.Shift, false, false, true, true) == GestureResult.Transfer);
Check("alt click pins", OrganizerRules.Evaluate(Modifier.Alt, Modifier.Alt, false, false, true, true) == GestureResult.Pin);
Check("alt drag never moves", OrganizerRules.Evaluate(Modifier.Alt, Modifier.Alt, false, true, false, true) == GestureResult.Cancel);
Check("shift drag is not quick transfer", OrganizerRules.Evaluate(Modifier.Shift, Modifier.Shift, false, true, false, true) == GestureResult.Drag);
Check("modifier release cancels", OrganizerRules.Evaluate(Modifier.Ctrl, Modifier.None, false, false, true, true) == GestureResult.Cancel);
Check("modifier change cancels", OrganizerRules.Evaluate(Modifier.Ctrl, Modifier.Shift, false, false, true, true) == GestureResult.Cancel);
Check("cancelled gesture cannot revive", OrganizerRules.Evaluate(Modifier.Ctrl, Modifier.Ctrl, true, false, true, true) == GestureResult.Cancel);
Check("release over other item cancels", OrganizerRules.Evaluate(Modifier.Ctrl, Modifier.Ctrl, false, false, true, false) == GestureResult.Cancel);
Check("combined modifiers cannot pin", OrganizerRules.Evaluate(Modifier.Alt | Modifier.Shift, Modifier.Alt | Modifier.Shift, false, false, true, true) == GestureResult.Cancel);

var item = new GameItemElement();
var target = new GameGridInventory { FreeCapacity = 7 };
Check("partial capacity moves 7 of 20", DirectTransfer.Move(item, target, out _) == 7 && item.unitCount == 13);
Check("full destination leaves source untouched", DirectTransfer.Move(item, target, out _) == 0 && item.unitCount == 13);
item = new GameItemElement { Pinned = true };
target = new GameGridInventory { FreeCapacity = 100 };
Check("pinned source cannot transfer", DirectTransfer.Move(item, target, out _) == 0 && target.FreeCapacity == 100);
item.Pinned = false; item.Cycle = true;
Check("self-descendant target rejected", DirectTransfer.Move(item, target, out _) == 0);
item.Cycle = false; target.Locked = true;
Check("locked target rejected", DirectTransfer.Move(item, target, out _) == 0);
target.Locked = false; target.Allowed = false;
Check("permission failure rejected", DirectTransfer.Move(item, target, out _) == 0);
target.Allowed = true; item.CanRemove = false;
Check("nonremovable source rejected", DirectTransfer.Move(item, target, out _) == 0);
item.CanRemove = true; item.parentInventory = target;
Check("same inventory rejected", DirectTransfer.Move(item, target, out _) == 0);
item.parentInventory = new GameInventory();
var nested = new GameGridInventory { FreeCapacity = 100 };
target.Redirect = nested;
Check("slot callback cannot redirect into nested container", DirectTransfer.Move(item, target, out _) == 0 && nested.FreeCapacity == 100);
target.Redirect = null; item.RejectCommit = true;
Check("failed commit terminates without retry loop", DirectTransfer.Move(item, target, out _) == 0 && item.unitCount == 20);
item.RejectCommit = false;
Check("whole-stack move counted once", DirectTransfer.Move(item, target, out _) == 20 && item.parentInventory == target);
item = new GameItemElement { Stackable = true };
target = new GameGridInventory { FreeCapacity = 0 };
target.childItems.Add(new GameItem { Stackable = true, AddCapacity = 3, parentInventory = target });
target.childItems.Add(new GameItem { Stackable = true, AddCapacity = 4, parentInventory = target });
Check("uses multiple direct-layer stack capacities", DirectTransfer.Move(item, target, out _) == 7 && item.unitCount == 13);
InspectionChecks.Run(Check);
ShortcutChecks.Run(Check);
ControlHintChecks.Run(Check);
StorageChecks.Run(Check);
Console.WriteLine($"{passed} checks passed. Unity runtime and game callbacks are not exercised by these doubles.");
