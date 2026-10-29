using System;
using System.Linq;
using HarmonyLib;

namespace StarterMod
{
    internal static class CompatibilityDiagnostics
    {
        internal static void Report()
        {
            foreach (var type in new[] { typeof(InputActionManager), typeof(ItemMouseDragHandler), typeof(ItemQuickTransferHandler), typeof(ItemQuickTransferAltHandler), typeof(ItemQuickTransferAltHandler1) })
            {
                var method = AccessTools.Method(type, type == typeof(InputActionManager) ? "Update" : type == typeof(ItemMouseDragHandler) ? "StartDrag" : "OnEventPress");
                var patches = Harmony.GetPatchInfo(method);
                if (patches == null) continue;
                var foreign = patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                    .Where(p => p.PatchMethod.DeclaringType?.Assembly != typeof(ModEntry).Assembly).Select(p => p.owner).Distinct().ToArray();
                if (foreign.Length > 0) ModEntry.Log?.Warning("Shared input patch (potential conflict): " + type.Name + " / " + string.Join(", ", foreign));
            }
        }
    }
}
