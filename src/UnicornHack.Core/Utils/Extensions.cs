using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils
{
    public static class Extensions
    {
        public static IReadOnlyCollection<T> GetFlags<T>(this T flags)
        {
            var values = new List<T>();
            var defaultValue = Enum.ToObject(typeof(T), value: 0);
            foreach (Enum currValue in Enum.GetValues(typeof(T)))
            {
                if (currValue.Equals(defaultValue))
                {
                    continue;
                }

                if (((Enum)(object)flags).HasFlag(currValue))
                {
                    values.Add((T)(object)currValue);
                }
            }

            return values;
        }

        public static IReadOnlyCollection<T> GetNonRedundantFlags<T>(this T flags, bool removeComposites)
        {
            var values = new HashSet<T>(flags.GetFlags());
            foreach (var currentValue in values.ToList())
            {
                var decomposedValues = currentValue.GetFlags();
                if (decomposedValues.Count > 1)
                {
                    if (removeComposites)
                    {
                        values.Remove(currentValue);
                    }
                    else
                    {
                        values.ExceptWith(decomposedValues.Where(v => !Equals(v, currentValue)));
                    }
                }
            }

            return values;
        }

        public static TList With<TList, T>(this TList list, T item)
            where TList : ICollection<T>
        {
            list.Add(item);
            return list;
        }

        public static TList AddRange<TList, T>(this TList list, IEnumerable<T> items)
            where TList : ICollection<T>
        {
            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
        }

        public static IEnumerable<Point> AsPoints(this IEnumerable<byte> bytes)
        {
            using (var enumerable = bytes.GetEnumerator())
            {
                while (enumerable.MoveNext())
                {
                    var x = enumerable.Current;
                    enumerable.MoveNext();
                    var y = enumerable.Current;
                    yield return new Point(x, y);
                }
            }
        }

        public static IEnumerable<byte> AsBytes(this IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                yield return point.X;
                yield return point.Y;
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            => dictionary.GetValueOrDefault(key, default(TValue));

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue fallBack)
            => dictionary.TryGetValue(key, out var value) ? value : fallBack;
    }
}