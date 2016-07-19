using System;
using System.Collections.Generic;
using System.Linq;

namespace Hex
{
    public static class Algorithms
    {
        // Breadth-first search for a symmetric neighbourhood relation
        public static IEnumerable<T> BFS<T>(IEnumerable<T> inits, IEnumerable<T> targets, Func<T, IEnumerable<T>> neighbours)
        {
            var targetSet = new HashSet<T>(targets);

            Dictionary<T, T> prevs = new Dictionary<T, T>();

            foreach (var p in inits)
                prevs.Add(p, p);

            var currentNodes = inits.ToList();

            while (currentNodes.Count > 0)
            {
                var nextNodes = new List<T>();
                foreach (var node in currentNodes)
                {
                    if (targetSet.Contains(node))
                    {
                        var reconstructedPath = new List<T>();
                        var n = node;
                        do
                        {
                            reconstructedPath.Add(n);
                            n = prevs[n];
                        } while ((object)prevs[n] != (object)n);
                        reconstructedPath.Add(n);
                        reconstructedPath.Reverse();
                        return reconstructedPath;

                    }
                    foreach (var next in neighbours(node))
                    {
                        if (!prevs.ContainsKey(next))
                        {
                            prevs.Add(next, node);
                            nextNodes.Add(next);
                        }
                    }
                }
                currentNodes = nextNodes;
            }

            return null;
        }

    }
}
