using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils
{
    public class SimpleRandom
    {
        private const float IntToFloat = 1.0f / 0x7FFFFFFF;
        private uint _x;

        public int Id { get; set; }

        public int Seed
        {
            get { return (int)_x; }
            set { _x = (uint)value; }
        }

        public virtual int Roll(int diceCount, int diceSides)
        {
            var result = 0;
            for (var i = 0; i < diceCount; i++)
            {
                result += Next(minValue: 0, maxValue: diceSides) + 1;
            }

            return result;
        }

        public virtual TInput Pick<TInput>(
            IReadOnlyList<TInput> items)
            => items[Next(0, items.Count)];

        public virtual TInput Pick<TInput>(
            IReadOnlyList<TInput> items,
            Func<TInput, bool> condition)
        {
            var index = Next(0, items.Count);
            for (var i = index; i < items.Count; i++)
            {
                var item = items[i];
                if (condition(item))
                {
                    return item;
                }
            }
            for (var i = 0; i < index; i++)
            {
                var item = items[i];
                if (condition(item))
                {
                    return item;
                }
            }
            throw new InvalidOperationException("No elements meet the condition");
        }

        public virtual TInput Pick<TInput>(
            IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight)
            => WeightedOrder(items, getWeight).First();

        public virtual TResult Pick<TInput, TResult>(
            IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight,
            Func<TInput, int, TResult> selector)
            => WeightedOrder(items, getWeight, selector).First();

        public virtual IEnumerable<TInput> WeightedOrder<TInput>(
            IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight)
            => WeightedOrder(items, getWeight, (item, index) => item);

        public virtual IEnumerable<TResult> WeightedOrder<TInput, TResult>(
            IReadOnlyList<TInput> items,
            Func<TInput, float> getWeight,
            Func<TInput, int, TResult> selector)
        {
            if (items == null
                || items.Count == 0)
            {
                throw new InvalidOperationException("Null or empty list of items");
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

        public int Next(int maxValue)
            => Next(minValue: 0, maxValue: maxValue);

        public int Next(int minValue, int maxValue)
            => (int)Next(minValue, (float)maxValue);

        public float Next(float minValue, float maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            var range = maxValue - minValue;
            if (range > 1 << 12 || range < 0)
            {
                throw new ArgumentOutOfRangeException($"Don't use this generator for ranges over {1 << 12}");
            }

            return minValue + IntToFloat * NextInt() * range;
        }

        public bool NextBool() => (0x80000000 & NextUInt()) == 0;

        private int NextInt()
            => (int)(0x7FFFFFFF & NextUInt());

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