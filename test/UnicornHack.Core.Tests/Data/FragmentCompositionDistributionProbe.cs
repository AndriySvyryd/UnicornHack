using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Data;

/// <summary>
///     Probe (Skip'd by default) that runs many full level generations across distinct seeds
///     to surface the distribution of Normal-fragment placements. Used to verify every
///     defined fragment is reachable (corner siblings and decorative quadrants in
///     particular) and that the smooth power-decay weight expressions stay in a sensible
///     range at higher instance counts. Re-enable by removing the Skip attribute and run
///     with `dotnet test --filter FullyQualifiedName~CompositionDistribution`.
/// </summary>
public class FragmentCompositionDistributionProbe(ITestOutputHelper output)
{
    // Watchlist of fragments that have historically been hard to place (S-edge corner
    // family). Each MUST appear at least once across the probe run.
    private static readonly string[] Watchlist =
    [
        "openCornerDoor",
        "pillarQuadrant",
        "twinPillarQuadrant",
        "notchedQuadrant",
        "trappedQuadrant",
        "corner"
    ];

    // Fragments that are intentionally branch-restricted and may legitimately not appear
    // when probing the Dungeon branch — exclude them from the "every fragment appears"
    // assertion but still report their counts.
    private static readonly HashSet<string> DungeonRestricted = new(StringComparer.Ordinal)
    {
        "irregularBlobRoom" // gated to non-dungeon, non-entrance branches.
    };

    [Fact(Skip = "Diagnostic probe; enable manually to inspect fragment placement distribution.")]
    public void All_fragments_appear_with_expected_frequency()
    {
        var totals = new Dictionary<string, int>(StringComparer.Ordinal);
        var variantTotals = new Dictionary<string, int>(StringComparer.Ordinal);
        var roomFailures = new Dictionary<string, int>(StringComparer.Ordinal);
        var weightSamples = new Dictionary<string, (float Min, float Max, int SampleCount)>(StringComparer.Ordinal);
        const int seedCount = 50;
        const int depthsPerSeed = 5;
        const int weightSampleEveryNLevels = 10;

        // Reference set of every Normal source fragment defined in data — drives both
        // the "all fragments appear" assertion and the periodic weight sampling.
        // `Loader.GetAll()` returns every rotated/flipped variant too, but stats are
        // keyed by SourceName and variants share the same GenerationWeight expression,
        // so we only need the source instances.
        var allFragments = NormalMapFragment.Loader.GetAll()
            .Where(f => f.Name == f.SourceName)
            .ToList();
        var allFragmentNames = allFragments.Select(f => f.Name).ToHashSet(StringComparer.Ordinal);

        var levelsCompleted = 0;
        var levelsFailed = 0;

        for (var s = 1u; s <= seedCount; s++)
        {
            var game = TestHelper.CreateGame(s);
            var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
            for (byte d = 1; d <= depthsPerSeed; d++)
            {
                var level = LevelGenerator.CreateEmpty(dungeon, depth: d, s, game.Manager);
                try
                {
                    LevelGenerator.EnsureGenerated(level);
                }
                catch (Exception ex)
                {
                    levelsFailed++;
                    output.WriteLine($"  [generation failure] seed={s} depth={d}");
                    for (Exception? cur = ex; cur is not null; cur = cur.InnerException)
                    {
                        output.WriteLine($"    {cur.GetType().FullName}: {cur.Message}");
                    }
                    continue;
                }

                foreach (var (name, count) in level.GenerationStats.FragmentInstancesByName)
                {
                    totals[name] = totals.GetValueOrDefault(name) + count;
                }

                foreach (var (name, count) in level.GenerationStats.FragmentInstancesByVariant)
                {
                    variantTotals[name] = variantTotals.GetValueOrDefault(name) + count;
                }

                foreach (var (name, count) in level.GenerationStats.FragmentRoomFailures)
                {
                    roomFailures[name] = roomFailures.GetValueOrDefault(name) + count;
                }

                levelsCompleted++;
                if (levelsCompleted % weightSampleEveryNLevels == 0)
                {
                    SampleFragmentWeights(allFragments, level, totals, weightSamples);
                }

                // Drain the ECS message queue between level generations so it doesn't
                // accumulate to the hard cap across many depths on the same manager.
                game.Manager.Queue.ProcessQueue(game.Manager);
            }
        }

        output.WriteLine(
            $"Generated {levelsCompleted}/{seedCount * depthsPerSeed} levels successfully ({levelsFailed} failed).");
        output.WriteLine("Fragment placement totals (sorted by count desc):");
        foreach (var (name, count) in totals.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
        {
            output.WriteLine($"  {count,5}  {name}");
        }

        var missing = allFragmentNames.Except(totals.Keys).OrderBy(n => n).ToList();
        if (missing.Count > 0)
        {
            output.WriteLine($"Fragments NEVER placed ({missing.Count}):");
            foreach (var name in missing)
            {
                output.WriteLine($"  {name}");
            }
        }

        if (variantTotals.Count > 0)
        {
            output.WriteLine("Fragment variant placement totals (Name incl. transform suffix, sorted by count desc):");
            foreach (var (name, count) in variantTotals.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
            {
                output.WriteLine($"  {count,5}  {name}");
            }

            // Variants that exist in the loader but were never placed. Variants are
            // derived from the source set via rotation/flip, so a missing variant points
            // at a directional asymmetry in the placement engine (e.g. a corner family
            // whose Rotate90 never lands).
            var allVariantNames = NormalMapFragment.Loader.GetAll()
                .Select(f => f.Name)
                .ToHashSet(StringComparer.Ordinal);
            var missingVariants = allVariantNames.Except(variantTotals.Keys).OrderBy(n => n).ToList();
            if (missingVariants.Count > 0)
            {
                output.WriteLine($"Variants NEVER placed ({missingVariants.Count}):");
                foreach (var name in missingVariants)
                {
                    output.WriteLine($"  {name}");
                }
            }
        }

        if (roomFailures.Count > 0)
        {
            output.WriteLine("Room generation failures attributed to fragments (sorted by count desc):");
            foreach (var (name, count) in roomFailures.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
            {
                output.WriteLine($"  {count,5}  {name}");
            }
        }

        output.WriteLine($"Sampled weights every {weightSampleEveryNLevels} levels:");
        foreach (var (name, (min, max, sampleCount)) in weightSamples
            .OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            output.WriteLine($"  {name,-30}  samples={sampleCount,3}  min={min,8:F4}  max={max,8:F4}");
        }

        // --- Assertions ---

        // 1. Every watchlist fragment must appear at least once.
        foreach (var name in Watchlist)
        {
            Assert.True(
                totals.GetValueOrDefault(name) > 0,
                $"Watchlist fragment '{name}' was never placed across {levelsCompleted} generations.");
        }

        // 2. Every defined fragment (except branch-restricted ones) must appear at
        //    least once. Branch-restricted fragments are still tracked but excluded
        //    from this assertion since the probe only generates Dungeon levels.
        var expected = allFragmentNames.Except(DungeonRestricted).ToList();
        var unmet = expected.Where(n => totals.GetValueOrDefault(n) <= 0).OrderBy(n => n).ToList();
        Assert.True(
            unmet.Count == 0,
            $"{unmet.Count} fragment(s) were never placed: {string.Join(", ", unmet)}");

        // 3. Every sampled weight must be non-negative and finite; the finite samples
        //    must be bounded above by a sane cap (the highest static weight in data is
        //    50 for AltarShrine / FeatureChamber / FeatureNiche, so 1000 leaves
        //    comfortable headroom).
        const float weightUpperBound = 1000f;
        foreach (var (name, (min, max, _)) in weightSamples)
        {
            Assert.False(float.IsNaN(min) || float.IsNaN(max),
                $"Fragment '{name}' produced a NaN weight (min={min}, max={max}).");
            Assert.False(float.IsInfinity(min) || float.IsInfinity(max),
                $"Fragment '{name}' produced an infinite weight (min={min}, max={max}).");
            Assert.True(min >= 0f,
                $"Fragment '{name}' produced a negative weight {min}.");
            Assert.True(max <= weightUpperBound,
                $"Fragment '{name}' produced an unexpectedly large finite weight {max} (> {weightUpperBound}).");
        }

        // 4. Decay-shaped fragments (anything that references $instances or
        //    $globalInstances in its weight expression) should still return a strictly
        //    positive weight at the observed instance counts — that's the whole point
        //    of switching to power decay. Fragments with a fixed weight or a
        //    branch-gated expression are exempt.
        foreach (var fragment in allFragments)
        {
            var expression = fragment.GenerationWeight;
            if (expression is null
                || DungeonRestricted.Contains(fragment.Name)
                || !(expression.Contains("$instances") || expression.Contains("$globalInstances")))
            {
                continue;
            }

            if (!weightSamples.TryGetValue(fragment.Name, out var range))
            {
                continue;
            }

            Assert.True(range.Min > 0f,
                $"Fragment '{fragment.Name}' weight dropped to {range.Min} despite using power decay; "
                + $"expression='{expression}'.");
        }
    }

    private static void SampleFragmentWeights(
        IReadOnlyList<NormalMapFragment> fragments,
        GameLevel level,
        Dictionary<string, int> totals,
        Dictionary<string, (float Min, float Max, int SampleCount)> weightSamples)
    {
        var branchName = level.Branch.Name;
        var depth = level.Depth;
        var gameStats = level.Game.GenerationStats;
        var levelStats = level.GenerationStats;

        foreach (var fragment in fragments)
        {
            float weight;
            try
            {
                weight = fragment.GetWeight(
                    branchName,
                    depth,
                    levelStats.FragmentInstancesByName,
                    gameStats.FragmentInstancesByName,
                    levelStats.FragmentInstancesByTag,
                    gameStats.FragmentInstancesByTag,
                    gameStats.FragmentInstancesByFeature);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Weight expression for '{fragment.Name}' failed to evaluate at depth {depth}: "
                    + $"{fragment.GenerationWeight}",
                    ex);
            }

            if (weightSamples.TryGetValue(fragment.Name, out var existing))
            {
                weightSamples[fragment.Name] = (
                    Math.Min(existing.Min, weight),
                    Math.Max(existing.Max, weight),
                    existing.SampleCount + 1);
            }
            else
            {
                weightSamples[fragment.Name] = (weight, weight, 1);
            }
        }
    }
}
