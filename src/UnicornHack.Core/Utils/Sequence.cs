using System.Collections.Generic;

namespace UnicornHack.Utils
{
    public static class Sequence
    {
        public static IEnumerable<int> GetAlternating(int initial, int min, int max)
        {
            yield return initial;
            var descending = initial - 1;
            var ascending = initial + 1;
            while (true)
            {
                var finished = true;
                if (descending >= min)
                {
                    finished = false;
                    yield return descending--;
                }
                if (ascending <= max)
                {
                    finished = false;
                    yield return ascending++;
                }
                if (finished)
                {
                    yield break;
                }
            }
        }
    }
}