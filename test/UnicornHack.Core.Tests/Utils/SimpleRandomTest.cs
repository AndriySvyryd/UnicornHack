using System.Text;

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
            var random = new SimpleRandom { Seed = seed };
            var selectedCounts = new int[itemWeights.Length];
            var selectionCount = 100000;
            for (var i = 0; i < selectionCount; i++)
            {
                var selectedIndex = random.Pick(itemWeights, f => f, (_, index) => index);
                selectedCounts[selectedIndex]++;
            }

            AssertDistribution(itemWeights, weightSum, selectedCounts, selectionCount, seed);
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
            var random = new SimpleRandom { Seed = seed };
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
            var random = new SimpleRandom { Seed = seed };
            var selectedCounts = new int[itemWeights.Length];
            var selectionCount = 1000000;
            for (var i = 0; i < selectionCount; i++)
            {
                var selectedIndex = random.NextBinomial(p, n);
                selectedCounts[selectedIndex]++;
            }

            AssertDistribution(itemWeights, weightSum, selectedCounts, selectionCount, seed);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(33)]
        [InlineData(50)]
        [InlineData(66)]
        [InlineData(70)]
        [InlineData(100)]
        public void NoStreakBool(int p)
        {
            var itemWeights = new[] { p, 100f - p };
            var maxPositiveStreak = 2;
            var maxNegativeStreak = 2;
            if (p == 0)
            {
                maxPositiveStreak = 0;
                maxNegativeStreak = int.MaxValue;
            }
            else if (p == 100)
            {
                maxPositiveStreak = int.MaxValue;
                maxNegativeStreak = 0;
            }
            else if (p > 50)
            {
                maxPositiveStreak = (int)Math.Ceiling((100f / (100 - p)) - 1) * 2;
            }
            else
            {
                maxNegativeStreak = (int)Math.Ceiling((100f / p) - 1) * 2;
            }

            var seed = (uint)Environment.TickCount;
            var random = new SimpleRandom { Seed = seed };
            var selectedCounts = new int[itemWeights.Length];
            var selectionCount = 1000000;
            var entropy = 0;
            var positiveStreak = 0;
            var negativeStreak = 0;
            for (var i = 0; i < selectionCount; i++)
            {
                var result = random.NextStreaklessPatternlessBool(p, ref entropy);
                if (result)
                {
                    selectedCounts[0]++;
                    positiveStreak++;

                    Assert.True(negativeStreak <= maxNegativeStreak,
                        $"Found negative streak of length {negativeStreak} at {i} for seed {seed}. Max streak expected {maxNegativeStreak}.");
                    negativeStreak = 0;
                }
                else
                {
                    selectedCounts[1]++;
                    negativeStreak++;

                    Assert.True(positiveStreak <= maxPositiveStreak,
                        $"Found positive streak of length {positiveStreak} at {i} for seed {seed}. Max streak expected {maxPositiveStreak}.");
                    positiveStreak = 0;
                }
            }

            AssertDistribution(itemWeights, 100f, selectedCounts, selectionCount, seed);
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

                if (!(Math.Abs(expected - actual) <= toleranceFraction * expected + uniformTolerance))
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(
                        $"Error at index {i}: expected {expected * selectionCount:N0}, "
                        + $"actual {actual * selectionCount:N0}\r\n"
                        + $"tolerance {(toleranceFraction * expected + uniformTolerance) * selectionCount:N0}, seed {seed}");

                    builder.AppendLine("Index Adjusted weight Actual frequency");
                    for (var j = 0; j < itemWeights.Length; j++)
                    {
                        builder.AppendLine(
                            $"{j,5:N0} {(itemWeights[j] / weightSum) * selectionCount,15:N0} {selectedCounts[j],16:N0}");
                    }

                    Assert.False(true, builder.ToString());
                }
            }
        }
    }
}
