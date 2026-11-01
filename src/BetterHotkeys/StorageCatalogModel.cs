using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterHotkeys
{
    internal sealed class CatalogOption
    {
        internal readonly string Id, Label;
        internal readonly string[] Groups;
        internal readonly CatalogSearch Search;
        internal CatalogOption(string id, string label, params string[] groups)
        { Id = id; Label = label; Groups = groups.Distinct(StringComparer.Ordinal).ToArray(); Search = new CatalogSearch(label); }
    }

    // The candidate filter never owns or rewrites the selected rules.
    internal sealed class StorageCatalogModel
    {
        internal readonly CatalogOption[] Options;
        internal readonly ICollection<string> Allowed, Denied;
        internal readonly HashSet<string> Expanded = new HashSet<string>(StringComparer.Ordinal);
        internal string Query { get; private set; } = "";
        internal HashSet<string>? Sample { get; private set; }
        internal StorageCatalogModel(IEnumerable<CatalogOption> options, ICollection<string> allowed, ICollection<string> denied)
        {
            Options = options.GroupBy(o => o.Id, StringComparer.Ordinal).Select(g => g.First())
                .OrderBy(o => o.Label, StringComparer.CurrentCulture).ToArray();
            Allowed = allowed; Denied = denied;
        }
        internal void Search(string query) { Query = query; Sample = null; }
        internal void Inspect(IEnumerable<string> ids) { Query = ""; Sample = new HashSet<string>(ids, StringComparer.Ordinal); }
        internal CatalogOption[] Candidates() => Options.Where(o => Sample != null ? Sample.Contains(o.Id) : o.Search.Matches(Query)).ToArray();
        internal CatalogOption[] Selected(bool denied) => Options.Where(o => (denied ? Denied : Allowed).Contains(o.Id)).ToArray();
        internal void Set(string id, bool denied, bool value)
        {
            var selected = denied ? Denied : Allowed;
            if (value) { if (!selected.Contains(id)) selected.Add(id); }
            else selected.Remove(id);
        }
        internal void Collect(bool denied)
        {
            if (Sample != null) foreach (var id in Sample) if (Options.Any(o => o.Id == id)) Set(id, denied, true);
        }
    }
}
