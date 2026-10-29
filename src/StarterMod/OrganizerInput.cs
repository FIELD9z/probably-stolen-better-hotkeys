using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarterMod
{
    internal static class OrganizerInput
    {
        private sealed class Gesture
        {
            internal GameItemElement Item = null!;
            internal GameInventory Source = null!;
            internal Modifier Modifier;
            internal Modifier PhysicalModifier;
            internal Vector3 Press;
            internal List<GameItemElement> Items = null!;
            internal int OtherSources;
            internal bool Cancelled;
            internal bool Dragging;
            internal bool PinValue;
        }

        private static Gesture? gesture;
        private static PixelWindow? targetWindow;
        private static GameGridInventory? targetGrid;
        private static readonly TargetHighlight highlight = new TargetHighlight();
        private static string lastNotice = "";
        private static float lastNoticeTime;
        internal static int SortedFrame { get; private set; } = -1;
        internal static void ConfigurationChanged()
        {
            gesture = null;
            SortedFrame = -1;
            if (!Settings.Current.Takeover || !Settings.Current.Transfer) ClearTarget();
            else if (targetWindow != null) highlight.Show(targetWindow);
        }

        private static Modifier ReadAction(bool notify)
        {
            if (!Settings.OwnsInput) return Modifier.None;
            var prefs = Settings.Current;
            var action = Modifier.None;
            void Match(Shortcut key, Modifier value, bool enabled)
            {
                if (enabled && RuntimeBindings.Held(key) && RuntimeBindings.Allowed(key, false, notify)) action |= value;
            }
            Match(prefs.Counter, Modifier.Shift, prefs.Transfer);
            Match(prefs.Container, Modifier.Ctrl, prefs.Transfer);
            Match(prefs.PinKey, Modifier.Alt, prefs.Pin);
            return action;
        }

        internal static void Reset()
        {
            InspectionOverlay.Reset();
            if (gesture?.Dragging == true && ItemMouseDragHandler.current != null)
                ItemMouseDragHandler.current.Cancel();
            gesture = null;
            ClearTarget();
            SortedFrame = -1;
        }

        private static void ClearTarget()
        {
            targetWindow = null; targetGrid = null;
            highlight.Dispose();
        }

        internal static Modifier ReadModifier()
        {
            var result = Modifier.None;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) result |= Modifier.Shift;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) result |= Modifier.Ctrl;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) result |= Modifier.Alt;
            return result;
        }

        // Called before the original manager dispatches any release/press events this frame.
        internal static bool BeforeInput(InputActionManager manager)
        {
            ModEntry.EnsureHooks();
            if (gesture == null && !Input.GetMouseButton(0) && !UIDragHandler.isDraggingAnyUI &&
                (ItemMouseDragHandler.current == null || !ItemMouseDragHandler.current.IsDraggingItem) &&
                (ItemMultiSelectHandler.current == null || !ItemMultiSelectHandler.current.IsGroupDragging)) Settings.ApplyPending();
            if (Settings.OwnsInput) GameAccess.ResetQuick(manager);
            if (!OrganizerWorld.InStore)
            {
                Reset();
                return true;
            }
            if (targetWindow != null && (!OrganizerWorld.IsContainer(targetWindow, out var grid) ||
                grid != targetGrid || targetGrid!.IsInsertLocked())) ClearTarget();
            highlight.Refresh();

            var blocked = OrganizerWorld.Blocked();
            try { InspectionOverlay.Tick(blocked); }
            catch (Exception error) { InspectionOverlay.Reset(); Settings.Current.Markers = false; ModEntry.Log?.Error("Marker module disabled: " + error); }
            var node = OrganizerWorld.PointerNode();
            var hovered = node?.FindParentElement() as GameItemElement;
            var scoped = OrganizerWorld.InScope(hovered);

            if (gesture != null) return ContinueGesture(blocked, hovered);
            if (blocked) return true;

            var window = node?.FindParentWindow();
            if (Settings.OwnsInput && Settings.Current.Transfer && Input.GetMouseButtonDown(0) && OrganizerWorld.IsContainer(window, out var clickedGrid) &&
                (!Settings.Current.HeaderOnly || OrganizerWorld.IsHeader(node, window!)))
            {
                if (targetWindow != window || targetGrid != clickedGrid)
                {
                    targetWindow = window; targetGrid = clickedGrid;
                    highlight.Show(window!);
                    if (Settings.Current.TargetNotice) Notice(Texts.T("转移目标：", "Transfer target: ") + window!.titleString);
                }
            }

            var dragging = ItemMouseDragHandler.current != null && ItemMouseDragHandler.current.IsDraggingItem;
            var groupDragging = ItemMultiSelectHandler.current != null && ItemMultiSelectHandler.current.IsGroupDragging;
            if (Settings.Current.Sort && !dragging && !groupDragging && !UIDragHandler.isDraggingAnyUI && window != null &&
                OrganizerWorld.IsHeader(node, window))
            {
                var sortPressed = Settings.Current.FollowSort ? KeyBindHelper.GetKeyDown("sort_menu") :
                    RuntimeBindings.Down(Settings.Current.SortKey);
                var sortGrid = sortPressed ? OrganizerWorld.DirectGrid(window) : null;
                if (sortPressed && sortGrid != null && OrganizerWorld.Classify(sortGrid) != Place.Unsupported &&
                    (Settings.Current.FollowSort || RuntimeBindings.Allowed(Settings.Current.SortKey, true)))
                {
                    SortedFrame = Time.frameCount;
                    try {
                        var sorted = InventorySortHelper.Sort(sortGrid, ModEntry.BigFirst, ModEntry.FromEnd);
                        Notice(sorted ? Texts.T("已整理：", "Sorted: ") + window.titleString : Texts.T("无法整理：空间或原生移动规则不允许", "Cannot sort: space or native movement rules prevent it."));
                    } catch (Exception error) { Settings.Current.Sort = false; ModEntry.Log?.Error("Sorting module disabled: " + error); }
                }
            }

            if (!Input.GetMouseButtonDown(0) || dragging || groupDragging) return true;
            if (!scoped) return true;
            var modifier = ReadAction(true);
            if (modifier == Modifier.None) return true;
            var selection = GameAccess.Selected();
            var useSelection = Settings.Current.Batch && selection.Contains(hovered!);
            var sameSource = useSelection ? selection.Where(i => i.parentInventory == hovered!.parentInventory).ToList()
                                          : new List<GameItemElement> { hovered! };
            gesture = new Gesture {
                Item = hovered!, Source = hovered!.parentInventory, Modifier = modifier, PhysicalModifier = ReadModifier(),
                Press = Input.mousePosition, Items = sameSource,
                OtherSources = useSelection ? selection.Count - sameSource.Count : 0,
                PinValue = !ItemPinHelper.IsPinned(hovered),
                Cancelled = !OrganizerRules.IsSingle(modifier)
            };
            if (!OrganizerRules.IsSingle(modifier)) Notice(Texts.T("多个快捷动作同时触发，已取消。", "Multiple shortcut actions matched; gesture cancelled."));
            return false;
        }

        private static bool ContinueGesture(bool blocked, GameItemElement? hovered)
        {
            var current = gesture!;
            var released = Input.GetMouseButtonUp(0);
            if (current.Dragging)
            {
                if (blocked || !OrganizerWorld.InScope(current.Item))
                {
                    ItemMouseDragHandler.current?.Cancel();
                    current.Cancelled = true; current.Dragging = false;
                    if (released || !Input.GetMouseButton(0)) gesture = null;
                    return false;
                }
                if (released) gesture = null;
                return true; // Native drag release commits its own validated placement, never a quick transfer.
            }

            if (blocked || !OrganizerWorld.InScope(current.Item) || current.Item.parentInventory != current.Source)
                current.Cancelled = true;
            var threshold = ItemMouseDragHandler.current != null ? ItemMouseDragHandler.current.dragThreshold : 10f;
            var crossed = (Input.mousePosition - current.Press).sqrMagnitude >= threshold * threshold;
            var result = OrganizerRules.Evaluate(current.Modifier, ReadAction(false), current.Cancelled,
                crossed, released, ReferenceEquals(hovered, current.Item));
            if (result == GestureResult.Cancel) current.Cancelled = true;
            if (result == GestureResult.Drag)
            {
                if (!released && !ItemPinHelper.IsPinned(current.Item))
                {
                    GameAccess.BeginDrag(current.Item, current.Press, current.PhysicalModifier);
                    current.Dragging = ItemMouseDragHandler.current != null && ItemMouseDragHandler.current.IsDraggingItem;
                    if (current.Dragging) return true;
                }
                current.Cancelled = true;
            }
            else if (result == GestureResult.Transfer || result == GestureResult.Pin)
            {
                if (result == GestureResult.Pin) Pin(current); else Transfer(current);
                GameAccess.RefreshSelected();
            }
            if (released || !Input.GetMouseButton(0)) gesture = null;
            return false;
        }

        private static void Pin(Gesture current)
        {
            var changed = 0; var skipped = current.OtherSources;
            foreach (var item in current.Items)
            {
                if (!OrganizerWorld.InScope(item) || item.parentInventory != current.Source || !ItemPinHelper.MayPin(item))
                { skipped++; continue; }
                ItemPinHelper.SetPinned(item, current.PinValue);
                changed++;
            }
            Notice(Texts.T($"{(current.PinValue ? "已固定" : "已解除固定")} {changed} 项；跳过 {skipped} 项（其他来源或不允许固定）。",
                $"{(current.PinValue ? "Pinned" : "Unpinned")} {changed} item(s); skipped {skipped} (other inventory or pinning not allowed)."));
        }

        private static void Transfer(Gesture current)
        {
            var destination = OrganizerRules.Destination(OrganizerWorld.Classify(current.Source), current.Modifier);
            GameGridInventory? target = destination == Place.Inventory ? EmporiumEntry.Instance.invElement :
                destination == Place.Counter ? EmporiumEntry.Instance.backInvinvElement :
                destination == Place.Container ? targetGrid : null;
            if (target == null)
            {
                TransferNotice(destination == Place.Container ? (Settings.Current.HeaderOnly
                    ? Texts.T("请先点击已打开的自有容器的标题栏，指定转移目标。", "Click the header of an open, owned container to select a transfer target first.")
                    : Texts.T("请先点击一个已打开的自有容器窗口，指定转移目标。", "Click an open, owned container window to select a transfer target first.")) :
                    Texts.T("此来源不支持该路线：柜台转移连接销售柜台，容器转移连接库存与容器。", "Unsupported route: counter transfer connects the sales counter; container transfer connects inventory and the selected container."), true);
                return;
            }
            var moved = 0; var remaining = 0; var skipped = current.OtherSources;
            var reasons = new HashSet<string>();
            // Grid Y grows downward in the vanilla placement scan; snapshot order before mutations.
            foreach (var item in current.Items.OrderBy(i => i.modifiedShape.minY).ThenBy(i => i.modifiedShape.minX).ThenBy(i => i.uniqueId).ToList())
            {
                if (!OrganizerWorld.InScope(item) || item.parentInventory != current.Source)
                { skipped++; reasons.Add(Texts.T("来源已变化或物品不可操作", "Source changed or item cannot be moved")); continue; }
                var requested = item.unitCount;
                var count = DirectTransfer.Move(item, target, out var reason);
                moved += count; remaining += Math.Max(0, requested - count);
                if (count == 0) skipped++;
                if (reason.Length > 0) reasons.Add(reason);
            }
            var targetName = destination == Place.Counter ? Texts.T("出售柜台", "Sales counter") : destination == Place.Inventory ? Texts.T("库存", "Inventory") : targetWindow!.titleString;
            TransferNotice(Texts.T($"目标：{targetName}。本次处理：转移 {moved} 个，未转移 {remaining} 个。未执行转移的物品堆：{skipped}（含其他来源）。", $"Target: {targetName}. Moved {moved} units; {remaining} units not moved among processed stacks. {skipped} stacks skipped (including other inventories). ") +
                (reasons.Count == 0 ? "" : Texts.T("原因：", "Reason: ") + string.Join("; ", reasons)), remaining != 0 || skipped != 0 || reasons.Count != 0);
        }

        private static void TransferNotice(string text, bool exception)
        {
            if (Settings.Current.Notices == "All" || (exception && Settings.Current.Notices == "Exceptions")) Notice(text);
        }

        internal static void Notice(string text)
        {
            if (text == lastNotice && Time.unscaledTime - lastNoticeTime < 1f) return;
            lastNotice = text; lastNoticeTime = Time.unscaledTime;
            // Notify takes message and palette ID, not title and body.
            StoreUIManager.Instance?.Notify(Texts.Name + ": " + text, "default");
        }
    }
}
