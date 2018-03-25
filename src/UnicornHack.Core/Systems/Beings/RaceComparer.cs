using System.Collections.Generic;

namespace UnicornHack.Systems.Beings
{
    public class RaceComparer : IComparer<(byte Level, int Id)>
    {
        public static readonly RaceComparer Instance = new RaceComparer();

        private RaceComparer()
        {
        }

        public int Compare((byte Level, int Id) x, (byte Level, int Id) y)
        {
            var diff = x.Level - y.Level;
            if (diff != 0)
            {
                return diff;
            }

            return x.Id - y.Id;
        }
    }
}
