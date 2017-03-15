using System;

namespace UnicornHack.Utils
{
    public class SimpleRandom
    {
        private const float IntToFloat = 1.0f / 0x7FFFFFFF;

        private uint _x;

        public SimpleRandom()
        {
            Seed = Environment.TickCount;
        }

        public SimpleRandom(int seed)
        {
            Seed = seed;
        }

        public int Seed
        {
            get { return (int)_x; }
            set { _x = (uint)value; }
        }

        public int Next(int maxValue)
            => Next(lowerBound: 0, maxValue: maxValue);

        public int Next(int lowerBound, int maxValue)
            => (int)Next(lowerBound, (float)maxValue);

        public float Next(float lowerBound, float maxValue)
        {
            if (lowerBound > maxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            var range = maxValue - lowerBound;
            if (range > 1 << 12 || range < 0)
            {
                throw new ArgumentOutOfRangeException("Don't use this implementation if the required range is over " +
                                                      (1 << 12));
            }

            return lowerBound + IntToFloat * NextInt() * range;
        }

        public bool NextBool() => (0x80000000 & NextUInt()) == 0;

        private int NextInt() => (int)(0x7FFFFFFF & NextUInt());

        private uint NextUInt()
        {
            if (_x == 0)
            {
                _x = 1;
            }
            var t = _x ^ (_x << 11);
            return _x = _x ^ (_x >> 19) ^ t ^ (t >> 8);
        }
    }
}