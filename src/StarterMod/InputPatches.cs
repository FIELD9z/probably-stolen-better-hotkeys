using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace StarterMod
{
    // The game's ModLoader applies these attributes before IMod.Init; every patch is inert until enabled.
    [HarmonyPatch(typeof(InputActionManager), "Update")]
    internal static class InputDispatchPatch
    {
        private static bool Prefix(InputActionManager __instance)
        {
            if (!ModEntry.Active) return true;
            try { return OrganizerInput.BeforeInput(__instance); }
            catch (Exception error) { ModEntry.Fail(error); return false; }
        }
    }

    [HarmonyPatch]
    internal static class QuickTransferEventPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var type in GameAccess.QuickTypes)
                foreach (var name in new[] { "OnEventPress", "OnEventRelease" })
                    yield return GameAccess.Method(type, name);
        }
        private static bool Prefix(EmptyActionHandler __instance)
        {
            if (!Settings.OwnsInput) return true;
            __instance.UpdateReset();
            return false;
        }
    }

    [HarmonyPatch]
    internal static class QuickTransferUpdatePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var type in GameAccess.QuickTypes) yield return GameAccess.Method(type, "OnEventUpdate");
        }
        private static bool Prefix(EmptyActionHandler __instance, ref bool __result)
        {
            if (!Settings.OwnsInput) return true;
            __instance.UpdateReset();
            __result = false; return false;
        }
    }

    [HarmonyPatch(typeof(ItemContextHandler), "OnEventPress")]
    internal static class HeaderSortConsumptionPatch
    {
        private static void Prefix(ref ISet<KeyCode> __0)
        {
            if (!ModEntry.Active || OrganizerInput.SortedFrame != Time.frameCount) return;
            var keys = new HashSet<KeyCode>(__0);
            keys.Remove(KeyBindHelper.Get("sort_menu"));
            __0 = keys;
        }
    }

    [HarmonyPatch(typeof(ItemMouseDragHandler), "StartDrag")]
    internal static class DragQuantityPatch
    {
        private static void Postfix(GameItemElement ___currentItem, bool ___isDragging,
            int ___maxRemove, ref int ___currentRemove)
        {
            if (!Settings.OwnsInput || !___isDragging || !OrganizerWorld.InScope(___currentItem)) return;
            // Do not retain vanilla's hardcoded LeftAlt=1 path when Alt is pressed during a normal gesture.
            var modifiers = OrganizerInput.ReadModifier();
            ___currentRemove = (modifiers & Modifier.Shift) != 0 ? (int)Math.Ceiling(___maxRemove / 2.0) : ___maxRemove;
            ___currentItem.UpdateFakeReal(___currentRemove);
        }
    }

    [HarmonyPatch(typeof(ItemContextHandler), "TriggerButton")]
    internal static class RememberMenuSortPatch
    {
        private static void Prefix(RichTextElement __0, GameGridInventory ___currentInventory, bool ___canSort,
            RichTextElement ___buttonSortBigStart, RichTextElement ___buttonSortSmallStart,
            RichTextElement ___buttonSortBigEnd, RichTextElement ___buttonSortSmallEnd)
        {
            if (!ModEntry.Active || ___currentInventory == null || !___canSort) return;
            if (__0 != ___buttonSortBigStart && __0 != ___buttonSortSmallStart &&
                __0 != ___buttonSortBigEnd && __0 != ___buttonSortSmallEnd) return;
            try { ModEntry.RememberSort(__0 == ___buttonSortBigStart || __0 == ___buttonSortBigEnd,
                __0 == ___buttonSortBigEnd || __0 == ___buttonSortSmallEnd); }
            catch (Exception error) { ModEntry.Log?.Warning("Cannot save sort preference: " + error.Message); }
        }
    }

    [HarmonyPatch(typeof(ModConfigWindow), "Apply")]
    internal static class ConfigFeedbackPatch
    {
        private static void Postfix(CustomUIWindow __0, ModConfig __1)
        {
            if (__1.ModId != Settings.Id || __0 == null || !__0.IsAlive) return;
            var note = __0.Get("restart_note");
            if (note != null && note.IsAlive) note.SetText(Settings.Status).SetVisible(true);
        }
    }
}
