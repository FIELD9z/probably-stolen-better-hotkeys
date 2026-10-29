using StarterMod;
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
        var p = new Preferences();
        var rows = ControlHintRows.Build(true, p, false, x => x, (_, _) => "");
        check("demand toggle remains advertised when disabled so it can be reenabled", rows.Any(r => r.Key == "H" && r.Action.Contains("demand")));
        check("automatic storage hint describes both gestures", rows.Any(r => r.Key.StartsWith("J /") && r.Action.Contains("selection")));
    }
}
