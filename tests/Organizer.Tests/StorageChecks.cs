using BetterHotkeys;
internal static class StorageChecks
{
    internal static void Run(Action<string, bool> check)
    {
        check("empty tags never auto-store even with match-all", !StorageMatchRules.Matches(Array.Empty<string>(), true, _ => true));
        check("match-any accepts a weapon without rare", StorageMatchRules.Matches(new[] { "weapon", "rare" }, false, t => t == "weapon"));
        check("match-all rejects a weapon without rare", !StorageMatchRules.Matches(new[] { "weapon", "rare" }, true, t => t == "weapon"));
        check("match-all accepts rare weapon", StorageMatchRules.Matches(new[] { "weapon", "rare" }, true, _ => true));
        var rule = new StorageRule { Name = "武器|箱,\n收藏 🔧", Tags = new[] { "type:gun", "condition:old", "type:gun" }, All = true, Priority = -9 };
        var read = StorageRuleCodec.Read(StorageRuleCodec.Write(rule))!;
        check("save preserves Unicode free name and separators", read.Name == rule.Name);
        check("save preserves mode priority and unique tags", read.All && read.Priority == -9 && read.Tags.SequenceEqual(new[] { "type:gun", "condition:old" }));
        check("unconfigured save stays unconfigured", StorageRuleCodec.Read("") == null);
        check("malformed saved rule never becomes accept-all", StorageRuleCodec.Read("bad|2|1|broken") == null);
        check("invalid encoded data rejected", StorageRuleCodec.Read("!|0|1|") == null);
        var item = new GameItemElement();
        var first = new GameGridInventory { FreeCapacity = 7 };
        var second = new GameGridInventory { FreeCapacity = 6 };
        var original = item.unitCount;
        var a = DirectTransfer.Move(item, first, out _);
        var b = DirectTransfer.Move(item, second, out _);
        check("stack split across pages preserves total and remainder", a == 7 && b == 6 && item.unitCount == original - 13);
        var specific = new StorageRule { AllowedItems = new[] { "drink" }, DeniedItems = new[] { "poison" }, Tags = new[] { "food" }, Priority = -100 };
        check("explicit item outranks every tag priority", StorageItemRules.Rank(specific, "drink", _ => false, false) == 0 && StorageItemRules.Rank(new StorageRule { Tags = new[] { "food" }, Priority = 999 }, "drink", _ => true, false) == 1);
        check("blacklist excludes matching tags and fallback", StorageItemRules.Rank(specific, "poison", _ => true, true) == -1);
        specific.DeniedItems = new[] { "drink" };
        check("blacklist overrides whitelist", StorageItemRules.Rank(specific, "drink", _ => true, true) == -1);
        check("mixed mode accepts unrelated item via tags", StorageItemRules.Rank(specific, "apple", _ => true, false) == 1);
        specific.ItemsOnly = true;
        check("items-only cannot fall through to tag or empty tier", StorageItemRules.Rank(specific, "apple", _ => true, true) == -1);
        check("fallback disabled by default", StorageItemRules.Rank(null, "apple", _ => true, false) == -1);
        check("empty-rule fallback uses last tier", StorageItemRules.Rank(new StorageRule { Name = "Spare" }, "apple", _ => false, true) == 2);
        check("deny-only container is not an empty fallback", StorageItemRules.Rank(new StorageRule { DeniedItems = new[] { "gun" } }, "apple", _ => false, true) == -1);
        check("plain manual insertion remains unrestricted", StorageItemRules.ManualAllowed(null, "apple", _ => false));
        check("manual rule check catches denied item", !StorageItemRules.ManualAllowed(specific, "drink", _ => true));
        check("empty items-only reports diagnostic", StorageItemRules.Diagnostic(new StorageRule { ItemsOnly = true }) == "EmptyWhitelist");
        check("fully denied whitelist reports diagnostic", StorageItemRules.Diagnostic(specific) == "DeniedWhitelist");
        check("plain unconfigured container does not report error", StorageItemRules.Diagnostic(new StorageRule()) == "");
        check("deny-only mixed mode reports no receivers", StorageItemRules.Diagnostic(new StorageRule { DeniedItems = new[] { "gun" } }) == "NoReceivers");
        var upgraded = StorageRuleCodec.Read(StorageRuleCodec.Write(specific))!;
        check("item rules roundtrip modes and ordered lists", upgraded.ItemsOnly && upgraded.AllowedItems.SequenceEqual(specific.AllowedItems) && upgraded.DeniedItems.SequenceEqual(specific.DeniedItems));
        var legacy = StorageRuleCodec.Read("VGVzdA==|0|5|dHlwZTpndW4=")!;
        check("legacy tag rule migrates without changing behavior", legacy.Name == "Test" && legacy.Priority == 5 && !legacy.ItemsOnly && legacy.AllowedItems.Length == 0 && legacy.Tags.Single() == "type:gun");
        check("invalid new mode does not become open fallback", StorageRuleCodec.Read("|0|0||2||") == null);
        var tagRule = new StorageRule { Tags = new[] { "food" }, DeniedTags = new[] { "illegal" }, AllowedItems = new[] { "apple" } };
        check("excluded tag overrides explicit allowed item", StorageItemRules.Rank(tagRule, "apple", _ => true, true) == -1);
        check("excluded tag overrides matching allowed tag", StorageItemRules.Rank(tagRule, "bread", _ => true, false) == -1);
        check("nonexcluded item still matches allowed tag", StorageItemRules.Rank(tagRule, "bread", t => t == "food", false) == 1);
        tagRule.ItemsOnly = true;
        check("excluded tag applies in items-only mode", StorageItemRules.Rank(tagRule, "apple", t => t == "illegal", false) == -1);
        check("manual rule prompt catches excluded tags", !StorageItemRules.ManualAllowed(tagRule, "apple", t => t == "illegal"));
        var denyOnly = new StorageRule { DeniedTags = new[] { "illegal" } };
        check("excluded tags count as container rules", StorageItemRules.HasLogic(denyOnly));
        check("deny-tag-only is not an empty fallback", StorageItemRules.Rank(denyOnly, "bread", _ => false, true) == -1);
        check("deny-tag-only reports no positive receiver", StorageItemRules.Diagnostic(denyOnly) == "NoReceivers");
        check("tag blacklist roundtrips", StorageRuleCodec.Read(StorageRuleCodec.Write(tagRule))!.DeniedTags.SequenceEqual(tagRule.DeniedTags));
        var oldItems = StorageRuleCodec.Read("|0|0||0||")!;
        check("seven-field rules retain empty tag blacklist", oldItems.DeniedTags.Length == 0 && !oldItems.ItemsOnly);
        check("four-field rules retain empty tag blacklist", legacy.DeniedTags.Length == 0);
        check("malformed tag blacklist fails closed", StorageRuleCodec.Read("|0|0||0|||!") == null);
        var p = new Preferences();
        var rows = ControlHintRows.Build(true, p, false, x => x, (_, _) => "");
        check("demand toggle remains advertised when disabled so it can be reenabled", rows.Any(r => r.Key == "D" && r.Action.Contains("demand")));
        check("automatic storage hint describes both gestures", rows.Any(r => r.Key.StartsWith("C /") && r.Action.Contains("selection")));
    }
}
