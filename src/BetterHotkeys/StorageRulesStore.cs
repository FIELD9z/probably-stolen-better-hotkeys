using System;
using System.Linq;
namespace BetterHotkeys
{
    internal static class StorageRulesStore
    {
        private const string Prefix = "storage-rule-v1:";
        internal static void Reset() { } // Reads always resolve against the current save.
        internal static StorageRule? Get(StorageTarget target) => PlayerStore.Instance == null || target.Key.Length == 0 ? null :
            StorageRuleCodec.Read(ModHelper.GetModData(Settings.Id, Prefix + target.Key, ""));
        internal static bool HasTags(GameItem owner)
        {
            if (PlayerStore.Instance == null || owner.uniqueId == 0) return false;
            var prefix = Prefix + owner.uniqueId + ":";
            return ModHelper.GetModDataKeys(Settings.Id).Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
                .Any(key => StorageItemRules.HasLogic(StorageRuleCodec.Read(ModHelper.GetModData(Settings.Id, key, ""))));
        }
        internal static void Set(StorageTarget target, StorageRule rule)
        {
            if (!StorageTargets.IsEligible(target)) throw new InvalidOperationException("Storage target is no longer available.");
            target.Owner.GetUniqueID();
            ModHelper.SetModData(Settings.Id, Prefix + target.Key, StorageRuleCodec.Write(rule));
        }
        internal static void Remove(StorageTarget target)
        {
            if (PlayerStore.Instance != null && target.Key.Length != 0) ModHelper.RemoveModData(Settings.Id, Prefix + target.Key);
        }
    }
}
