using System.Collections.Generic;

namespace UnicornHack.Utils
{
    internal static class Extensions
    {
        public static TList With<TList, T>(this TList list, T item)
            where TList : ICollection<T>
        {
            list.Add(item);
            return list;
        }
    }
}