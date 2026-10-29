using System;
using System.Collections.Generic;
using System.Linq;

namespace StarterMod
{
    internal sealed class StorageTagOption
    {
        internal string Id { get; }
        internal string Label { get; }
        internal StorageTagOption(string id, string label) { Id = id; Label = label; }
    }

    internal static class StorageTagCatalog
    {
        private static ItemCondition? VisibleCondition(ItemFeature feature)
        {
            // Same visibility gate as NegociationUIManager.AddFeatureSection.
            if (feature.isDisabled || !feature.IsInitiallyShown() ||
                (feature.GetPublicHidden() && !feature.IsExposed()) || !feature.useCondition) return null;
            var condition = feature.IsExposed() ? feature.realCondition : feature.fakeCondition;
            return condition == null || string.IsNullOrEmpty(condition.identifier) ||
                string.IsNullOrWhiteSpace(condition.display) ? null : condition;
        }

        internal static List<StorageTagOption> Options()
        {
            var choices = new Dictionary<string, StorageTagOption>(StringComparer.Ordinal);
            foreach (var type in TypeHelper.ALL_TYPES_LIST)
                choices["type:" + type] = new StorageTagOption("type:" + type, TypeHelper.GetTypeDisplayName(type));
            foreach (var conditions in ItemConditionList.conditionLookup.Values)
                foreach (var condition in conditions)
                    if (!string.IsNullOrEmpty(condition.identifier) && !string.IsNullOrWhiteSpace(condition.display))
                        choices["condition:" + condition.identifier] = new StorageTagOption("condition:" + condition.identifier, condition.display);
            foreach (var pair in FixedFeatures)
                choices["feature:" + pair.Key] = new StorageTagOption("feature:" + pair.Key, LocHelper.GetLocalizedMechanic(pair.Value, Array.Empty<object>()));
            foreach (var state in new[] { "stolen", "contraband", "disabled", "contraband-low", "contraband-mid", "contraband-high", "contraband-critical" })
                choices["state:" + state] = new StorageTagOption("state:" + state, Label("state:" + state));
            if (OrganizerWorld.InStore)
            {
                // Only public conditions on owned items; never create or expose undiscovered features.
                var seen = new HashSet<GraphNodeStorage>();
                void Visit(GraphNodeStorage node)
                {
                    if (!seen.Add(node)) return;
                    if (node is GameItem item)
                    {
                        if (item.isDebugMenu || !GeneralHelper.IsItemOwned(item) || GeneralHelper.IsInsideNonOwnedContainer(item)) return;
                        foreach (var feature in item.itemFeatures)
                        {
                            var condition = VisibleCondition(feature);
                            if (condition == null) continue;
                            var id = "condition:" + condition.identifier;
                            choices[id] = new StorageTagOption(id, condition.display);
                        }
                    }
                    foreach (var child in node.children) if (child != null) Visit(child);
                }
                Visit(EmporiumEntry.Instance.invElement);
            }
            return choices.Values.OrderBy(option => option.Label, StringComparer.CurrentCulture).ToList();
        }

        internal static bool Matches(GameItem item, string id)
        {
            if (id == "state:stolen") return item.IsTag("STOLEN_TAG");
            if (id == "state:contraband") return ContrabandHelper.IsContraband(item);
            if (id == "state:disabled") return item.forceDisableActivate || item.forceDisableUse;
            if (id.StartsWith("state:contraband-", StringComparison.Ordinal))
                return ContrabandHelper.IsContraband(item) && item.IsTag("CONTRABAND_LEVEL") &&
                    item.GetTagReadonly("CONTRABAND_LEVEL").GetString() == "CONTRABAND_LEVEL_" + id.Substring(17).ToUpperInvariant();
            if (id.StartsWith("type:", StringComparison.Ordinal))
                return item.GetGameItemType().Contains(id.Substring(5));
            if (id.StartsWith("feature:", StringComparison.Ordinal))
            {
                var featureId = id.Substring(8);
                return FixedFeatures.ContainsKey(featureId) && item.itemFeatures.Any(feature =>
                    feature.identifier == featureId && !feature.isDisabled && feature.IsInitiallyShown() &&
                    (!feature.GetPublicHidden() || feature.IsExposed()));
            }
            if (!id.StartsWith("condition:", StringComparison.Ordinal)) return false;
            var conditionId = id.Substring(10);
            foreach (var feature in item.itemFeatures)
                if (VisibleCondition(feature)?.identifier == conditionId) return true;
            return false;
        }

        // Verified constructors use a single public, non-deceptive meaning for these IDs.
        private static readonly Dictionary<string, string> FixedFeatures = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["chilled"] = "item_feature_chilled",
            ["frozen"] = "item_feature_frozen",
            ["iceCold"] = "item_feature_ice_cold"
        };

        internal static string Label(string id)
        {
            if (id.StartsWith("type:", StringComparison.Ordinal)) return TypeHelper.GetTypeDisplayName(id.Substring(5));
            if (id.StartsWith("condition:", StringComparison.Ordinal))
            {
                var key = id.Substring(10);
                foreach (var group in ItemConditionList.conditionLookup.Values)
                    foreach (var condition in group)
                        if (condition.identifier == key) return condition.display;
            }
            if (id.StartsWith("feature:", StringComparison.Ordinal) && FixedFeatures.TryGetValue(id.Substring(8), out var locKey))
                return LocHelper.GetLocalizedMechanic(locKey, Array.Empty<object>());
            if (id == "state:stolen") return Texts.T("赃物", "Stolen");
            if (id == "state:contraband") return TypeHelper.GetTypeDisplayName("CONTRABAND");
            if (id == "state:disabled") return LocHelper.GetLocalizedMechanic("mech_tooltip_disabled", Array.Empty<object>());
            if (id.StartsWith("state:contraband-", StringComparison.Ordinal))
                return LocHelper.GetLocalizedMechanic("mech_contraband_" + id.Substring(17), Array.Empty<object>());
            return Texts.T("不可用属性", "Unavailable attribute");
        }
    }
}
