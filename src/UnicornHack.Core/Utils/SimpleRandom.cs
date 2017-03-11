using System;

namespace UnicornHack.Utils
{
    public class SimpleRandom
    {
        private const float IntToFloat = 1.0f / Int32.MaxValue;

        private uint _x;

        public SimpleRandom()
        {
            Reseed(Environment.TickCount);
        }

        public SimpleRandom(int seed)
        {
            Reseed(seed);
        }

        public int GetSeed()
        {
            return (int)_x;
        }

        public void Reseed(int seed)
        {
            _x = (uint)seed;
        }

        public int Next(int maxValue)
        {
            return Next(lowerBound: 0, maxValue: maxValue);
        }

        public int Next(int lowerBound, int maxValue)
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

            return lowerBound + (int)(IntToFloat * NextInt() * range);
        }

        private uint NextUInt()
        {
            if (_x == 0)
            {
                _x = 1;
            }
            var t = _x ^ (_x << 11);
            return _x = _x ^ (_x >> 19) ^ t ^ (t >> 8);
        }

        private int NextInt()
        {
            return (int)(0x7FFFFFFF & NextUInt());
        }

        public bool NextBool()
        {
            return (0x80000000 & NextUInt()) == 0;
        }
    }
}