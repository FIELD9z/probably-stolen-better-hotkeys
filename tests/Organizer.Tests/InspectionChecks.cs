using BetterHotkeys;

internal static class InspectionChecks
{
    internal static void Run(Action<string, bool> check)
    {
        var tap = new DoubleTap();
        check("single B does not toggle", !tap.Update(1f, true, true));
        check("held B does not toggle", !tap.Update(1.1f, false, true));
        check("second B within interval toggles", tap.Update(1.2f, true, true));
        check("third B starts a new pair", !tap.Update(1.3f, true, true));
        check("fourth B toggles back", tap.Update(1.4f, true, true));
        tap.Update(2f, true, true);
        check("slow second press starts a new pair", !tap.Update(2.5f, true, true));
        check("new pair can complete", tap.Update(2.7f, true, true));
        tap.Update(3f, true, true);
        tap.Update(3.1f, false, false);
        check("blocked frame cancels pending double tap", !tap.Update(3.2f, true, true));
        check("blocked key cannot toggle", !tap.Update(3.3f, true, false));
        check("input after blocking begins fresh", !tap.Update(3.4f, true, true));
        tap.Reset();
        check("lifecycle reset cancels pending tap", !tap.Update(3.5f, true, true));

        var edges = new Dictionary<string, string[]> {
            ["window"] = new[] { "bag", "clean", "direct" },
            ["bag"] = new[] { "tabs" },
            ["tabs"] = new[] { "page1", "page2" },
            ["page1"] = new[] { "box" },
            ["box"] = new[] { "stolen" },
            ["page2"] = new[] { "illegal" },
        };
        var own = new Dictionary<string, ItemRisk> {
            ["stolen"] = ItemRisk.Stolen, ["illegal"] = ItemRisk.Contraband,
            ["direct"] = ItemRisk.Stolen | ItemRisk.Contraband,
        };
        var reads = new Dictionary<string, int>();
        Dictionary<string, ItemRisk> Scan(params string[] roots) => InspectionRules.Aggregate(roots,
            node => edges.GetValueOrDefault(node, Array.Empty<string>()), node => {
                reads[node] = reads.GetValueOrDefault(node) + 1;
                return own.GetValueOrDefault(node);
            });
        var result = Scan("window");
        check("nested stolen flag reaches outer bag", result["bag"].HasFlag(ItemRisk.Stolen));
        check("hidden-page contraband reaches outer bag", result["bag"].HasFlag(ItemRisk.Contraband));
        check("inner box keeps only its own descendant category", result["box"] == ItemRisk.Stolen);
        check("clean sibling is not contaminated by parent risk", result["clean"] == ItemRisk.None);
        check("direct dual-category item retains both flags", result["direct"] == (ItemRisk.Stolen | ItemRisk.Contraband));
        check("scanning leaves original flags unchanged", own.Count == 3 && !own.ContainsKey("bag"));

        edges["box"] = new[] { "stolen", "bag" }; // Malformed cyclic graph must terminate.
        reads.Clear();
        result = Scan("window", "page2", "window");
        check("cycle and duplicate roots read each node once", reads.Values.All(count => count == 1));
        check("flags propagate across shared cyclic graph", result["box"] == (ItemRisk.Stolen | ItemRisk.Contraband));
        edges["box"] = new[] { "stolen" };
        own.Remove("stolen");
        result = Scan("window");
        check("refresh clears stale stolen marks", result["box"] == ItemRisk.None && result["bag"] == ItemRisk.Contraband);
        check("empty roots produce no marks", Scan().Count == 0);
    }
}
