using System.Collections.Generic;

namespace UnicornHack.Utils
{
    public static class Extensions
    {
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
    }
}