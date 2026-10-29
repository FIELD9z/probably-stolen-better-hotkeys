using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterHotkeys
{
    // Explicit private members verified against Build 26263409. Missing members fail at enable time.
    internal static class GameAccess
    {
        internal static FieldInfo Field(Type type, string name) => AccessTools.Field(type, name)
            ?? throw new MissingFieldException(type.FullName, name);
        internal static MethodInfo Method(Type type, string name) => AccessTools.Method(type, name)
            ?? throw new MissingMethodException(type.FullName, name);

        internal static readonly FieldInfo Selection = Field(typeof(ItemMultiSelectHandler), "selectedItems");
        internal static readonly MethodInfo Prune = Method(typeof(ItemMultiSelectHandler), "PruneSelection");
        internal static readonly MethodInfo RefreshSelection = Method(typeof(ItemMultiSelectHandler), "RefreshSelectionHighlight");
        internal static readonly FieldInfo Title = Field(typeof(PixelWindow), "topTitleBackground");
        internal static readonly FieldInfo DragItem = Field(typeof(ItemMouseDragHandler), "currentItem");
        internal static readonly FieldInfo DragPosition = Field(typeof(ItemMouseDragHandler), "lastMouseClickedPosition");
        internal static readonly FieldInfo DragKey = Field(typeof(ItemMouseDragHandler), "lastMouseClickedKey");
        internal static readonly FieldInfo DragAmount = Field(typeof(ItemMouseDragHandler), "currentRemove");
        internal static readonly FieldInfo DragMax = Field(typeof(ItemMouseDragHandler), "maxRemove");
        internal static readonly MethodInfo StartDrag = Method(typeof(ItemMouseDragHandler), "StartDrag");
        internal static readonly FieldInfo MouseBlocks = Field(typeof(OverlayHandler), "mouseBlockStacks");
        internal static readonly FieldInfo FadeBlock = Field(typeof(OverlayHandler), "isFadeBlock");

        internal static List<GameItemElement> Selected()
        {
            var handler = ItemMultiSelectHandler.current;
            if (handler == null) return new List<GameItemElement>();
            Prune.Invoke(handler, null);
            return new List<GameItemElement>((List<GameItemElement>)Selection.GetValue(handler));
        }

        internal static void RefreshSelected()
        {
            var handler = ItemMultiSelectHandler.current;
            if (handler == null) return;
            Prune.Invoke(handler, null);
            RefreshSelection.Invoke(handler, null);
        }

        internal static void BeginDrag(GameItemElement item, Vector3 pressPosition, Modifier modifier)
        {
            var handler = ItemMouseDragHandler.current;
            if (handler == null || !item.MayRemove() || ItemPinHelper.IsPinned(item)) return;
            DragItem.SetValue(handler, item);
            DragPosition.SetValue(handler, pressPosition);
            DragKey.SetValue(handler, KeyCode.Mouse0);
            StartDrag.Invoke(handler, null);
            // Native StartDrag only checks left Shift. Honor right Shift as well.
            if (handler.IsDraggingItem && modifier == Modifier.Shift)
            {
                var amount = (int)Math.Ceiling((int)DragMax.GetValue(handler) / 2.0);
                DragAmount.SetValue(handler, amount);
                item.UpdateFakeReal(amount);
            }
        }

        internal static TreeNodeRender? TitleNode(PixelWindow window) => Title.GetValue(window) as TreeNodeRender;
        internal static bool IsOverlayBlocking()
        {
            var overlay = OverlayHandler.current;
            return overlay != null && ((int)MouseBlocks.GetValue(overlay) > 0 || (bool)FadeBlock.GetValue(overlay));
        }

        internal static readonly Type[] QuickTypes = {
            typeof(ItemQuickTransferHandler), typeof(ItemQuickTransferAltHandler), typeof(ItemQuickTransferAltHandler1)
        };
        internal static void ResetQuick(InputActionManager manager)
        {
            foreach (var handler in manager.actionHandlers)
                if (Array.IndexOf(QuickTypes, handler.GetType()) >= 0) handler.UpdateReset();
        }

    }
}
