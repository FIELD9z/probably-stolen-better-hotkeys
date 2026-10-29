using System;
using UnityEngine;

namespace BetterHotkeys
{
    // Modal sampling only: never selects, detaches, drags, or mutates the sampled game item.
    internal static class StorageItemPicker
    {
        private static StorageTarget? target;
        private static PlayerStore? run;
        private static Action<GameItemElement>? picked;
        private static Action<bool>? finished;
        private static bool ending, validEnd;
        private static bool consumingItemClick, navigationClick;
        private static int startedFrame;
        internal static bool IsPicking => target != null;

        internal static bool TargetValid(StorageTarget candidate) => OrganizerWorld.InStore &&
            StorageTargets.IsEligible(candidate) && candidate.Window.IsVisible() &&
            candidate.Window.handler != null && candidate.Window.handler.gameObject.activeInHierarchy;

        internal static void Start(StorageTarget destination, Action<GameItemElement> onPicked, Action<bool> onFinished)
        {
            Reset(); target = destination; run = PlayerStore.Instance;
            picked = onPicked; finished = onFinished; startedFrame = Time.frameCount;
            OrganizerInput.Notice(Texts.T("点选模式：连续左键点击已打开容器中的物品；右键完成并返回面板。不会移动物品。", "Pick items from open containers with left clicks; right click returns. Items will not move."));
        }
        internal static void Reset() { target = null; run = null; picked = null; finished = null; ending = validEnd = consumingItemClick = navigationClick = false; }

        internal static bool BeforeInput()
        {
            if (target == null) return true;
            var valid = ModEntry.Active && ReferenceEquals(run, PlayerStore.Instance) && TargetValid(target);
            if (!valid) { ending = true; validEnd = false; }
            if (!ending && Input.GetMouseButtonDown(1)) { ending = true; validEnd = true; }
            if (ending)
            {
                // Native tooltip updates stop while the editor owns input. Clear both the
                // cached listener and the visible overlay before returning to either editor path.
                ItemTooltipHandler.current?.UpdateReset();
                OverlayHandler.current?.HideTooltip();
                // Retain ownership through every release, including the right click used to finish.
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) return false;
                var callback = finished; var resume = valid && validEnd;
                Reset(); callback?.Invoke(resume); return false;
            }
            if (Time.frameCount == startedFrame) return false;
            if (consumingItemClick)
            {
                if (!Input.GetMouseButton(0)) consumingItemClick = false;
                return false;
            }
            if (navigationClick)
            {
                if (!Input.GetMouseButton(0)) navigationClick = false;
                return true; // A non-item press must reach its matching native release.
            }
            if (OrganizerWorld.Blocked()) return false;
            if (UIDragHandler.isDraggingAnyUI || (ItemMouseDragHandler.current?.IsDraggingItem ?? false) ||
                (ItemMultiSelectHandler.current?.IsGroupDragging ?? false)) return false;
            if (Input.GetMouseButtonDown(0))
            {
                // PointerNode uses the first raycast hit: a foreground UI cannot be clicked through.
                var node = OrganizerWorld.PointerNode();
                var item = node?.FindParentElement() as GameItemElement;
                if (item != null)
                {
                    consumingItemClick = true;
                    if (OrganizerWorld.InScope(item) && item.handler?.FindParentWindow()?.IsVisible() == true &&
                        item.handler.gameObject.activeInHierarchy && !string.IsNullOrEmpty(item.identifier))
                    { StorageItemDisplay.Observe(item); picked?.Invoke(item); }
                    return false;
                }
                // Native page tabs and window headers retain their ordinary click handling.
                navigationClick = true;
                return true;
            }
            // Idle native updates establish hovered SelectableElement state before a tab click.
            // Keyboard actions are modal, so a shortcut cannot transfer an existing selection.
            return !Input.anyKey;
        }
    }
}
