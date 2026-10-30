using System;
using System.Collections.Generic;

namespace WindBot.Game.AI.Decks
{
    // Our own copies of the LINQ operators the bots use. The Android app (libWindbot) is built with the linker, which
    // strips any System.Linq / System.Core method WindBot itself never calls (Enumerable.ThenByDescending crashed the
    // bots there), so CustomBots.dll must not depend on them. These live in the bots' namespace, so they bind ahead of
    // System.Linq, and only use List<T>, Comparison<T> and Func<> from mscorlib.
    public static class MiniLinq
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var result = new List<T>();
            foreach (var x in source) if (predicate(x)) result.Add(x);
            return result;
        }

        public static IEnumerable<TResult> Select<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        {
            var result = new List<TResult>();
            foreach (var x in source) result.Add(selector(x));
            return result;
        }

        public static IEnumerable<TResult> SelectMany<T, TResult>(this IEnumerable<T> source, Func<T, IEnumerable<TResult>> selector)
        {
            var result = new List<TResult>();
            foreach (var x in source) result.AddRange(selector(x));
            return result;
        }

        public static bool Any<T>(this IEnumerable<T> source)
        {
            foreach (var x in source) return true;
            return false;
        }

        public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var x in source) if (predicate(x)) return true;
            return false;
        }

        public static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var x in source) if (!predicate(x)) return false;
            return true;
        }

        public static int Count<T>(this IEnumerable<T> source)
        {
            int n = 0;
            foreach (var x in source) n++;
            return n;
        }

        public static int Count<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int n = 0;
            foreach (var x in source) if (predicate(x)) n++;
            return n;
        }

        public static T First<T>(this IEnumerable<T> source)
        {
            foreach (var x in source) return x;
            throw new InvalidOperationException("Sequence contains no elements");
        }

        public static T First<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var x in source) if (predicate(x)) return x;
            throw new InvalidOperationException("Sequence contains no matching element");
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source)
        {
            foreach (var x in source) return x;
            return default(T);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var x in source) if (predicate(x)) return x;
            return default(T);
        }

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            return new List<T>(source);
        }

        public static T[] ToArray<T>(this IEnumerable<T> source)
        {
            return new List<T>(source).ToArray();
        }

        public static bool Contains<T>(this IEnumerable<T> source, T value)
        {
            var eq = EqualityComparer<T>.Default;
            foreach (var x in source) if (eq.Equals(x, value)) return true;
            return false;
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> source, int count)
        {
            var result = new List<T>();
            if (count <= 0) return result;
            foreach (var x in source)
            {
                result.Add(x);
                if (result.Count >= count) break;
            }
            return result;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var result = new List<T>(first);
            result.AddRange(second);
            return result;
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source)
        {
            var result = new List<T>();
            foreach (var x in source) if (!result.Contains(x)) result.Add(x);
            return result;
        }

        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> source)
        {
            var items = new List<T>(source);
            var result = new List<T>();
            for (int i = items.Count - 1; i >= 0; i--) result.Add(items[i]);
            return result;
        }

        public static IEnumerable<T> DefaultIfEmpty<T>(this IEnumerable<T> source, T defaultValue)
        {
            var result = new List<T>(source);
            if (result.Count == 0) result.Add(defaultValue);
            return result;
        }

        public static IEnumerable<IList<T>> GroupBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> key)
        {
            var keys = new List<TKey>();
            var groups = new List<IList<T>>();
            var eq = EqualityComparer<TKey>.Default;
            foreach (var x in source)
            {
                var k = key(x);
                int i = -1;
                for (int j = 0; j < keys.Count; j++) if (eq.Equals(keys[j], k)) { i = j; break; }
                if (i < 0) { keys.Add(k); groups.Add(new List<T> { x }); }
                else groups[i].Add(x);
            }
            return groups;
        }

        public static int Sum<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            int total = 0;
            foreach (var x in source) total += selector(x);
            return total;
        }

        public static int Max(this IEnumerable<int> source)
        {
            bool any = false;
            int best = 0;
            foreach (var x in source) { if (!any || x > best) best = x; any = true; }
            if (!any) throw new InvalidOperationException("Sequence contains no elements");
            return best;
        }

        public static int Max<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            return source.Select(selector).Max();
        }

        public static IEnumerable<int> Range(int start, int count)
        {
            var result = new List<int>();
            for (int i = 0; i < count; i++) result.Add(start + i);
            return result;
        }

        // ---------- ordering (stable, like LINQ) ----------

        public static Ordered<T> OrderBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> key)
        {
            return new Ordered<T>(source).Then(key, false);
        }

        public static Ordered<T> OrderByDescending<T, TKey>(this IEnumerable<T> source, Func<T, TKey> key)
        {
            return new Ordered<T>(source).Then(key, true);
        }

        public static Ordered<T> ThenBy<T, TKey>(this Ordered<T> source, Func<T, TKey> key)
        {
            return source.Then(key, false);
        }

        public static Ordered<T> ThenByDescending<T, TKey>(this Ordered<T> source, Func<T, TKey> key)
        {
            return source.Then(key, true);
        }
    }

    // A sorted sequence that can take further ThenBy keys; sorts when enumerated.
    public class Ordered<T> : IEnumerable<T>
    {
        readonly List<T> items;
        readonly List<Comparison<T>> keys = new List<Comparison<T>>();

        public Ordered(IEnumerable<T> source)
        {
            items = new List<T>(source);
        }

        Ordered(List<T> items, List<Comparison<T>> keys)
        {
            this.items = items;
            this.keys = keys;
        }

        public Ordered<T> Then<TKey>(Func<T, TKey> key, bool descending)
        {
            var cmp = Comparer<TKey>.Default;
            var next = new List<Comparison<T>>(keys);
            next.Add(descending ? (Comparison<T>)((a, b) => cmp.Compare(key(b), key(a))) : (a, b) => cmp.Compare(key(a), key(b)));
            return new Ordered<T>(items, next);
        }

        List<T> Sorted()
        {
            // stable: break ties by original position
            var indexed = new List<KeyValuePair<int, T>>();
            for (int i = 0; i < items.Count; i++) indexed.Add(new KeyValuePair<int, T>(i, items[i]));
            indexed.Sort((a, b) =>
            {
                foreach (var k in keys)
                {
                    int c = k(a.Value, b.Value);
                    if (c != 0) return c;
                }
                return a.Key.CompareTo(b.Key);
            });
            var result = new List<T>();
            foreach (var p in indexed) result.Add(p.Value);
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Sorted().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
