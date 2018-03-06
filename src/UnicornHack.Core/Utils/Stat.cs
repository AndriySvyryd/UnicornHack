using System;
using static System.Math;

namespace UnicornHack.Utils
{
    public static class Stat
    {
        public static long BinomialCoefficient(long n, long k)
        {
            if (n < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (k < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (k > n)
            {
                return 0;
            }

            if (n == k)
            {
                return 1;
            }

            if (k > n - k)
            {
                k = n - k;
            }

            long c = 1;
            for (long i = 1; i <= k; i++)
            {
                c *= n--;
                c /= i;
            }

            return c;
        }

        public static double BinomialDistributionMass(long k, long n, double p) =>
            Pow(p, k) * BinomialCoefficient(n, k) * Pow(1 - p, n - k);
    }
}