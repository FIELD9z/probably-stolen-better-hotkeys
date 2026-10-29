using System;
using System.Collections.Generic;
using System.Linq;
namespace BetterHotkeys
{
    internal static class StorageMatchRules
    {
        internal static bool Matches(IEnumerable<string> tags, bool all, Func<string, bool> match)
        {
            var values = tags.ToArray();
            return values.Length != 0 && (all ? values.All(match) : values.Any(match));
        }
    }
}
