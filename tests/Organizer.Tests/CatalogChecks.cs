using BetterHotkeys;

internal static class CatalogChecks
{
    internal static void Run(Action<string, bool> check)
    {
        var gun = new CatalogSearch("<color=#fff>精制手枪</color>");
        check("Chinese search ignores rich-text markup", gun.Matches("手枪") && !gun.Matches("color"));
        check("initials match a Chinese substring", gun.Matches("sq") && gun.Matches("JZSQ"));
        check("full pinyin matches a Chinese substring", gun.Matches("shouqiang") && gun.Matches("jing zhi shou qiang"));
        check("partial pinyin supports typing ahead", gun.Matches("shouqi"));
        check("mixed Chinese and pinyin match", gun.Matches("精制sq"));
        check("unrelated search is rejected", !gun.Matches("pingguo") && !gun.Matches("jzqq"));
        check("polyphonic characters accept both relevant readings", new CatalogSearch("重庆").Matches("chongqing") && new CatalogSearch("重量").Matches("zhongliang"));
        check("polyphonic initials are not limited to the first reading", new CatalogSearch("银行").Matches("yh") && new CatalogSearch("行走").Matches("xz"));
        check("umlaut accepts v, u-colon and marked vowels", new CatalogSearch("铝").Matches("lv") && new CatalogSearch("铝").Matches("lü") && new CatalogSearch("铝").Matches("lu:"));
        check("mixed Latin digits and Chinese remain searchable", new CatalogSearch("AK-47 弹药").Matches("ak47dy") && new CatalogSearch("AK-47 弹药").Matches("47danyao"));
        check("English search remains case-insensitive", new CatalogSearch("Medical Tool").Matches("MEDICAL") && new CatalogSearch("Medical Tool").Matches("medical tool"));
        check("empty query includes all candidates", gun.Matches(" "));
        check("supplementary CJK text remains searchable", new CatalogSearch("𠀀测试").Matches("𠀀cs"));
        var longName = new CatalogSearch(string.Concat(Enumerable.Repeat("重行长乐", 12)));
        check("polyphonic matching does not truncate combinations", longName.Matches(string.Concat(Enumerable.Repeat("zxcl", 12))));

        var allowed = new List<string> { "gun", "old" };
        var denied = new HashSet<string> { "water" };
        var catalog = new StorageCatalogModel(new[] {
            new CatalogOption("gun", "手枪", "武器", "工具"),
            new CatalogOption("water", "饮用水", "水"),
            new CatalogOption("old", "旧名单物品", "其他"),
            new CatalogOption("gun", "手枪", "武器")
        }, allowed, denied);
        catalog.Search("sq");
        check("search crosses collapsed groups and deduplicates items", catalog.Expanded.Count == 0 && catalog.Candidates().Select(o => o.Id).SequenceEqual(new[] { "gun" }));
        check("search does not hide selected rules", catalog.Selected(false).Length == 2 && catalog.Selected(true).Single().Id == "water");
        catalog.Set("old", false, false);
        check("filtered-out selected entry can be removed", allowed.SequenceEqual(new[] { "gun" }));
        catalog.Inspect(new[] { "water" });
        check("sample replaces text filter without changing selected rules", catalog.Query == "" && catalog.Candidates().Single().Id == "water" && allowed.Contains("gun"));
        catalog.Collect(false); catalog.Collect(false);
        check("collect appends once and preserves prior rule order", allowed.SequenceEqual(new[] { "gun", "water" }));
        check("allow and exclude can coexist for existing deny-wins logic", catalog.Selected(false).Any(o => o.Id == "water") && catalog.Selected(true).Any(o => o.Id == "water"));
        catalog.Search("sq");
        check("typing search exits sample filter", catalog.Sample == null && catalog.Candidates().Single().Id == "gun");
        denied.Clear();
        check("clear one group preserves the other", catalog.Selected(true).Length == 0 && catalog.Selected(false).Length == 2);
        catalog.Search("");
        check("reset search retains full candidate catalog", catalog.Candidates().Length == 3);
        check("cross-category item retains native memberships", catalog.Options.Single(o => o.Id == "gun").Groups.Length == 2);
    }
}
