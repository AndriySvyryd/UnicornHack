using System;
using System.Linq;
using System.Text;
using Xunit;

namespace UnicornHack.Utils
{
    public class SimpleRandomTest
    {
        [Fact]
        public void Pick_uniform()
        {
            var weightSum = 0f;
            var items = new float[10];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = 1;
                weightSum += items[i];
            }

            Test(items, weightSum);
        }

        [Fact]
        public void Pick_clustered()
        {
            var weightSum = 0f;
            var items = new float[10];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = i < 5 ? 0 : 1;
                weightSum += items[i];
            }

            Test(items, weightSum);
        }

        [Fact]
        public void Pick_alternating()
        {
            var weightSum = 0f;
            var items = new float[10];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = i % 2;
                weightSum += items[i];
            }

            Test(items, weightSum);
        }

        [Fact]
        public void Pick_increasing()
        {
            var weightSum = 1f;
            var items = new float[10];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = weightSum;
                weightSum += items[i];
            }

            Test(items, weightSum);
        }

        private void Test(float[] items, float weightSum)
        {
            var seed = Environment.TickCount;
            var random = new SimpleRandom {Seed = seed};
            var selectedCounts = new int[items.Length];
            var selectionCount = 10000;
            for (var i = 0; i < selectionCount; i++)
            {
                var selectedIndex = random.Pick(items, f => f, (f, index) => index);
                selectedCounts[selectedIndex]++;
            }

            var toleranceFraction = items.Length / Math.Sqrt(selectionCount);
            var uniformTolerance = toleranceFraction / (items.Length * 10);
            for (var i = 0; i < items.Length; i++)
            {
                var expected = (double)items[i] / weightSum;
                var actual = (double)selectedCounts[i] / selectionCount;

                if (!(Math.Abs(expected - actual) <= toleranceFraction * expected + uniformTolerance))
                {
                    var builder = new StringBuilder();

                    builder.Append(
                        $"Expected {expected}, actual {actual}, tolerance {toleranceFraction * expected} at index {i}, seed {seed}");

                    builder.AppendLine("\tIndex:\tfrequency,\tweight:");
                    for (var j = 0; j < items.Length; j++)
                    {
                        builder.AppendLine($"\t{j}:\t{selectedCounts[j]},\t{items[j]}");
                    }

                    Assert.False(true, builder.ToString());
                }
            }
        }

        [Fact]
        public void Pick_predicate()
        {
            var weightSum = 0f;
            var items = new float[10];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = 1;
                weightSum += items[i];
            }

            var seed = Environment.TickCount;
            var random = new SimpleRandom {Seed = seed};
            var selectedCounts = new int[items.Length];
            var selectionCount = 10000;
            for (var i = 0; i < selectionCount; i++)
            {
                var orderedItems = random.WeightedOrder(items, f => f, (f, index) => (f, index));
                var selectedIndex = orderedItems.First(tuple => tuple.Item2 % 2 == 0).Item2;
                selectedCounts[selectedIndex]++;
            }

            var tolerance = 1.5 * items.Length / Math.Sqrt(selectionCount);
            for (var i = 0; i < items.Length; i++)
            {
                var expected = i % 2 == 0 ? 2.0 * items[i] / weightSum : 0;
                var actual = (double)selectedCounts[i] / selectionCount;

                if (!(Math.Abs(expected - actual) <= tolerance * expected))
                {
                    var builder = new StringBuilder();

                    builder.Append(
                        $"Expected {expected}, actual {actual}, tolerance {tolerance * expected} at index {i}, seed {seed}");

                    builder.AppendLine("\tIndex:\tfrequency,\tweight:");
                    for (var j = 0; j < items.Length; j++)
                    {
                        builder.AppendLine($"\t{j}:\t{selectedCounts[j]},\t{items[j]}");
                    }

                    Assert.False(true, builder.ToString());
                }
            }
        }
    }
}