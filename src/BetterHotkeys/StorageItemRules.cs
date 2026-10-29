using System;
using System.Linq;
namespace BetterHotkeys
{
    internal static class StorageItemRules
    {
        // Rank precedes every numeric priority. A blacklist can never fall through to another rank.
        internal static int Rank(StorageRule? rule, string itemId, Func<string, bool> matchTag, bool fallback)
        {
            if (rule != null)
            {
                if (rule.DeniedItems.Contains(itemId, StringComparer.Ordinal)) return -1;
                if (rule.AllowedItems.Contains(itemId, StringComparer.Ordinal)) return 0;
                if (rule.ItemsOnly) return -1;
                if (StorageMatchRules.Matches(rule.Tags, rule.All, matchTag)) return 1;
            }
            return fallback && !HasLogic(rule) ? 2 : -1;
        }
        internal static bool HasLogic(StorageRule? rule) => rule != null &&
            (rule.ItemsOnly || rule.Tags.Length != 0 || rule.AllowedItems.Length != 0 || rule.DeniedItems.Length != 0);
        internal static bool ManualAllowed(StorageRule? rule, string itemId, Func<string, bool> matchTag) =>
            !HasLogic(rule) || Rank(rule, itemId, matchTag, false) >= 0;
        internal static string Diagnostic(StorageRule? rule)
        {
            if (rule == null || !HasLogic(rule)) return "";
            if (rule.ItemsOnly && rule.AllowedItems.Length == 0) return "EmptyWhitelist";
            var hasAllowed = rule.AllowedItems.Except(rule.DeniedItems, StringComparer.Ordinal).Any();
            if (rule.ItemsOnly && !hasAllowed) return "DeniedWhitelist";
            if (!rule.ItemsOnly && rule.Tags.Length == 0 && !hasAllowed) return "NoReceivers";
            return "";
        }
    }
}
