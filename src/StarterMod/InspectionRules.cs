using System;
using System.Collections.Generic;

namespace StarterMod
{
    [Flags]
    internal enum ItemRisk { None = 0, Stolen = 1, Contraband = 2 }

    internal sealed class DoubleTap
    {
        private float? previous;
        internal void Reset() => previous = null;
        internal bool Update(float now, bool pressed, bool eligible, float interval = 0.35f)
        {
            if (!eligible) { Reset(); return false; }
            if (!pressed) return false;
            var elapsed = previous.HasValue ? now - previous.Value : float.PositiveInfinity;
            if (elapsed >= 0 && elapsed <= interval) { Reset(); return true; }
            previous = now;
            return false;
        }
    }

    internal static class InspectionRules
    {
        // Reverse propagation includes every nested page and terminates even on shared/cyclic graphs.
        internal static Dictionary<T, ItemRisk> Aggregate<T>(IEnumerable<T> roots,
            Func<T, IEnumerable<T>> children, Func<T, ItemRisk> direct) where T : class
        {
            var risks = new Dictionary<T, ItemRisk>();
            var parents = new Dictionary<T, HashSet<T>>();
            var pending = new Queue<T>(roots);
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                if (node == null || risks.ContainsKey(node)) continue;
                risks.Add(node, direct(node));
                foreach (var child in children(node))
                {
                    if (child == null) continue;
                    if (!parents.TryGetValue(child, out var owners)) parents.Add(child, owners = new HashSet<T>());
                    owners.Add(node);
                    if (!risks.ContainsKey(child)) pending.Enqueue(child);
                }
            }
            foreach (var pair in risks) if (pair.Value != ItemRisk.None) pending.Enqueue(pair.Key);
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                if (!parents.TryGetValue(node, out var owners)) continue;
                foreach (var owner in owners)
                {
                    var combined = risks[owner] | risks[node];
                    if (combined == risks[owner]) continue;
                    risks[owner] = combined;
                    pending.Enqueue(owner);
                }
            }
            return risks;
        }
    }
}
