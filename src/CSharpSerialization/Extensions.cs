using System;
using System.Collections.Generic;

namespace CSharpSerialization
{
    public static class Extensions
    {
        public static IEnumerable<T> GetFlags<T>(this T flags)
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
    }
}