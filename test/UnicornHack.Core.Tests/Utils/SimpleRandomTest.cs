using System;
using System.Linq;
using System.Text;
using Xunit;
using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;
using TheoryAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;
using InlineDataAttribute = DummyDataAttribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class DummyDataAttribute : Attribute
{
    public DummyDataAttribute(params object[] data)
    {
    }
}

namespace UnicornHack.Utils
{
    // These tests are flaky
    public class SimpleRandomTest
    {
        [Fact]
        public void Pick_uniform()
        {
            var weightSum = 0f;
            var itemWeights = new float[10];
            for (var i = 0; i < itemWeights.Length; i++)
            {
                itemWeights[i] = 1;
                weightSum += itemWeights[i];
            }

            TestPick(itemWeights, weightSum);
        }

        [Fact]
        public void Pick_clustered()
        {
            var weightSum = 0f;
            var itemWeights = new float[10];
            for (var i = 0; i < itemWeights.Length; i++)
            {
                itemWeights[i] = i < 5 ? 0 : 1;
                weightSum += itemWeights[i];
            }

            TestPick(itemWeights, weightSum);
        }

        [Fact]
        public void Pick_alternating()
        {
            var weightSum = 0f;
            var itemWeights = new float[10];
            for (var i = 0; i < itemWeights.Length; i++)
            {
                itemWeights[i] = i % 2;
                weightSum += itemWeights[i];
            }

            TestPick(itemWeights, weightSum);
        }

        [Fact]
        public void Pick_increasing()
        {
            var weightSum = 1f;
            var itemWeights = new float[10];
            for (var i = 0; i < itemWeights.Length; i++)
            {
                itemWeights[i] = weightSum;
                weightSum += itemWeights[i];
            }

            TestPick(itemWeights, weightSum);
        }

        private void TestPick(float[] itemWeights, float weightSum)
        {
            var seed = (uint)Environment.TickCount;
            var random = new SimpleRandom {Seed = seed};
            var selectedCounts = new int[itemWeights.Length];
            var selectionCount = 100000;
            for (var i = 0; i < selectionCount; i++)
            {
                var selectedIndex = random.Pick(itemWeights, f => f, (f, index) => index);
                selectedCounts[selectedIndex]++;
            }

            AssertDistribution(itemWeights, weightSum, selectedCounts, selectionCount, seed);
        }

        [Theory]
        [InlineData(0.0f)]
        [InlineData(0.2f)]
        [InlineData(0.5f)]
        [InlineData(0.7f)]
        [InlineData(1.0f)]
        public void Binomial(float p)
        {
            var weightSum = 0f;
            var itemWeights = new float[10];
            var n = itemWeights.Length - 1;
            for (var i = 0; i < itemWeights.Length; i++)
            {
                itemWeights[i] = (float)Stat.BinomialDistributionMass(i, n, p);
                weightSum += itemWeights[i];
            }

            var seed = (uint)Environment.TickCount;
            var random = new SimpleRandom {Seed = seed};
            var selectedCounts = new int[itemWeights.Length];
            var selectionCount = 500000;
            for (var i = 0; i < selectionCount; i++)
            {
                var selectedIndex = random.NextBinomial(p, n);
                selectedCounts[selectedIndex]++;
            }

            AssertDistribution(itemWeights, weightSum, selectedCounts, selectionCount, seed);
        }

        private static void AssertDistribution(
            float[] itemWeights, float weightSum, int[] selectedCounts, int selectionCount, uint seed)
        {
            var toleranceFraction = itemWeights.Length / Math.Sqrt(selectionCount);
            var uniformTolerance = toleranceFraction / (itemWeights.Length * 10);
            for (var i = 0; i < itemWeights.Length; i++)
            {
                var expected = (double)itemWeights[i] / weightSum;
                var actual = (double)selectedCounts[i] / selectionCount;

                if (!(Math.Abs(expected - actual) <= toleranceFraction * expected + uniformTolerance)) //
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(
                        $"Error at index {i}: expected {expected * selectionCount:N4}, "
                        + $"actual {actual * selectionCount:N4}\r\n"
                        + $"tolerance {(toleranceFraction * expected + uniformTolerance) * selectionCount:N4}, seed {seed}");

                    builder.AppendLine("\tIndex\tFrequency\tAdjusted weight");
                    for (var j = 0; j < itemWeights.Length; j++)
                    {
                        builder.AppendLine(
                            $"\t{j,4:N0}\t{selectedCounts[j],9:D} {(itemWeights[j] / weightSum) * selectionCount,12:N4}");
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

            var seed = (uint)Environment.TickCount;
            var random = new SimpleRandom {Seed = seed};
            var selectedCounts = new int[items.Length];
            var selectionCount = 10000;
            for (var i = 0; i < selectionCount; i++)
            {
                var orderedItems = random.WeightedOrder(items, f => f, (f, index) => (f, index));
                var selectedIndex = orderedItems.First(tuple => tuple.index % 2 == 0).index;
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
