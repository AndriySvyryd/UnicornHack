using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     Implementation of a xorshift pseudo-random number generator with period 2^32-1.
    /// </summary>
    public class SimpleRandom : NotificationEntity
    {
        private const float IntToFloat = 1.0f / int.MaxValue;

        public uint Seed { get; set; } = 1;

        public int Roll(int diceCount, int diceSides)
        {
            var result = 0;
            for (var i = 0; i < diceCount; i++)
            {
                result += Next(minValue: 0, maxValue: diceSides) + 1;
            }

            return result;
        }

        public TInput Pick<TInput>(IReadOnlyList<TInput> items) => items[Next(0, items.Count)];

        public TInput Pick<TInput>(IReadOnlyList<TInput> items, Func<TInput, bool> condition) =>
            !TryPick(items, condition, out var item)
                ? throw new InvalidOperationException("No elements meet the condition")
                : item;

        public TInput TryPick<TInput>(IReadOnlyList<TInput> items) => items[Next(0, items.Count)];

        public bool TryPick<TInput>(IReadOnlyList<TInput> items, Func<TInput, bool> condition, out TInput item)
        {
            var index = Next(0, items.Count);
            for (var i = index; i < items.Count; i++)
            {
                item = items[i];
                if (condition(item))
                {
                    return true;
                }
            }

            for (var i = 0; i < index; i++)
            {
                item = items[i];
                if (condition(item))
                {
                    return true;
                }
            }

            item = default;
            return false;
        }

        public TInput Pick<TInput>(IReadOnlyList<TInput> items, Func<TInput, float> getWeight) =>
            WeightedOrder(items, getWeight).First();

        public TResult Pick<TInput, TResult>(IReadOnlyList<TInput> items, Func<TInput, float> getWeight,
            Func<TInput, int, TResult> selector) => WeightedOrder(items, getWeight, selector).First();

        public IEnumerable<TInput> WeightedOrder<TInput>(IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight) => WeightedOrder(items, getWeight, (item, index) => item);

        public IEnumerable<TResult> WeightedOrder<TInput, TResult>(IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight, Func<TInput, int, TResult> selector)
        {
            if (items == null || items.Count == 0)
            {
                yield break;
            }

            var cumulativeWeights = new float[items.Count];
            var sum = 0f;
            var indexToProcess = 0;
            while (true)
            {
                var selectedIndex = -1;
                for (; indexToProcess < items.Count; indexToProcess++)
                {
                    var item = items[indexToProcess];
                    var weight = getWeight(item);
                    if (float.IsPositiveInfinity(weight))
                    {
                        selectedIndex = indexToProcess;
                        break;
                    }

                    if (weight < 0)
                    {
                        weight = 0;
                    }

                    sum += weight;
                    cumulativeWeights[indexToProcess] = sum;
                }

                if (selectedIndex == -1)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (sum == 0)
                    {
                        yield break;
                    }

                    selectedIndex = BinarySearch(cumulativeWeights, Next(0, sum));
                }

                yield return selector(items[selectedIndex], selectedIndex);

                var selectedWeight = cumulativeWeights[selectedIndex];
                if (selectedIndex > 0)
                {
                    selectedWeight = selectedWeight - cumulativeWeights[selectedIndex - 1];
                }

                if (selectedIndex == indexToProcess)
                {
                    cumulativeWeights[indexToProcess] = sum;
                    indexToProcess++;
                }
                else
                {
                    for (var i = selectedIndex; i < items.Count; i++)
                    {
                        cumulativeWeights[i] -= selectedWeight;
                    }
                }

                sum -= selectedWeight;
            }
        }

        private static int BinarySearch(float[] numbers, float target)
        {
            Debug.Assert(numbers.Length > 0);

            var first = 0;
            var last = numbers.Length - 1;
            while (first < last)
            {
                var midPoint = (first + last) / 2;
                var midValue = numbers[midPoint];

                // ReSharper disable once CompareOfFloatsByEqualityOperator

                if (target > midValue)
                {
                    first = midPoint + 1;
                }
                else if (target < midValue)
                {
                    last = midPoint;
                }
                else
                {
                    break;
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            while (first < numbers.Length && numbers[first] == 0)
            {
                first++;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            while (first > 0 && target == numbers[first - 1])
            {
                first--;
            }

            return first == numbers.Length ? first - 1 : first;
        }

        /// <summary>
        ///     Returns a random number in the range [0, <paramref name="maxValue" />)
        /// </summary>
        public int Next(int maxValue) => Next(minValue: 0, maxValue);

        /// <summary>
        ///     Returns a random number in the range [<paramref name="minValue" />, <paramref name="maxValue" />)
        /// </summary>
        public int Next(int minValue, int maxValue) => (int)Next(minValue, (float)maxValue);

        public float Next(float minValue, float maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            var range = maxValue - minValue;
            if (range > int.MaxValue || range < 0)
            {
                throw new ArgumentOutOfRangeException($"Don't use this generator for ranges over {int.MaxValue}");
            }

            return minValue + IntToFloat * NextInt() * range;
        }

        private int NextInt() => (int)(int.MaxValue & NextUInt());

        /// <summary>
        ///     Generates <paramref name="n" /> random numbers and returns how many of them were smaller than <paramref name="p" />
        /// </summary>
        /// <param name="p"> A number between 0 and 1 </param>
        /// <param name="n"> The number of numbers to generate </param>
        /// <returns></returns>
        public int NextBinomial(float p, int n)
        {
            var successes = 0;
            for (var i = 0; i < n; i++)
            {
                if (Next(0, 1f) <= p)
                {
                    successes++;
                }
            }

            return successes;
        }

        private uint NextUInt()
        {
            Seed ^= Seed << 13;
            Seed ^= Seed >> 17;
            Seed ^= Seed << 5;
            return Seed;
        }

        public bool NextBool() => (0x80000000 & NextUInt()) == 0;

        private const int BoolSeedMask = 0x000000FF;
        private const int PositiveStreakShift = 8;
        private const int PositiveStreakMask = 0x0000FF00;
        private const int NegativeStreakShift = 16;
        private const int NegativeStreakMask = 0x00FF0000;
        private const int InitializedFlag = 0x01000000;

        /// <summary>
        ///     Returns a random bool with the given <paramref name="successProbability" /> without any streaks
        ///     longer than ((100 / p) - 1) * 2 and with no predictable order.
        /// </summary>
        /// <param name="successProbability">Percent of expected <c>true</c> results.</param>
        /// <param name="entropyState">State for streak-breaking.</param>
        /// <returns> A random bool. </returns>
        public virtual bool NextBool(int successProbability, ref int entropyState)
        {
            if (successProbability < 0
                || successProbability > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(successProbability));
            }

            switch (successProbability)
            {
                case 0:
                    return false;
                case 100:
                    return true;
            }

            var positiveStreak = (entropyState & PositiveStreakMask) >> PositiveStreakShift;
            var negativeStreak = (entropyState & NegativeStreakMask) >> NegativeStreakShift;
            var seed = entropyState & BoolSeedMask;
            if (entropyState == 0)
            {
                positiveStreak = Next(100);
                negativeStreak = Next(100);
                seed = Next(100);
            }

            if (positiveStreak < 100)
            {
                positiveStreak += successProbability;
            }

            if (negativeStreak < 100)
            {
                negativeStreak += 100 - successProbability;
            }

            seed += successProbability;

            var result = seed >= 100;

            if (positiveStreak >= 100
                && negativeStreak >= 100)
            {
                positiveStreak = 0;
                negativeStreak = 0;
                seed = Next(100);
            }
            else if (result)
            {
                seed -= 100;
            }

            entropyState = seed
                           | (positiveStreak << PositiveStreakShift)
                           | (negativeStreak << NegativeStreakShift)
                           | InitializedFlag;

            return result;
        }
    }
}
