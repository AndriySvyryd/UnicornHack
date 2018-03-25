using System.Collections.Generic;

namespace UnicornHack.Systems.Time
{
    public class TickComparer : IComparer<(int Tick, int Id)>
    {
        public static readonly TickComparer Instance = new TickComparer();

        private TickComparer()
        {
        }

        public int Compare((int Tick, int Id) x, (int Tick, int Id) y)
        {
            var diff = x.Tick - y.Tick;
            if (diff != 0)
            {
                return diff;
            }

            return x.Id - y.Id;
        }
    }
}
