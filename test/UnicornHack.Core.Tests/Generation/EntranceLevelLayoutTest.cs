using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public class EntranceLevelLayoutTest
{
    [Fact]
    public void Never_assigns_deep_tier_rewards()
    {
        var layout = new EntranceLevelLayout();
        var parameters = layout.GetParameters(EntranceDifficulty);

        for (uint seed = 1; seed <= 200; seed++)
        {
            var graph = layout.BuildGraph(
                parameters, new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                continue;
            }

            foreach (var room in graph.Rooms)
            {
                Assert.True((room.Features & RoomFeatures.BigFountain) == 0,
                    $"BigFountain assigned on seed {seed}");
                Assert.True((room.Features & RoomFeatures.MajorShrine) == 0,
                    $"MajorShrine assigned on seed {seed}");
                Assert.True((room.Features & RoomFeatures.RitualShrine) == 0,
                    $"RitualShrine assigned on seed {seed}");
            }
        }
    }

    [Fact]
    public void Caps_at_most_one_altar_and_one_race_pedestal_per_level()
    {
        var layout = new EntranceLevelLayout();
        var parameters = layout.GetParameters(EntranceDifficulty);

        for (uint seed = 1; seed <= 200; seed++)
        {
            var graph = layout.BuildGraph(
                parameters, new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                continue;
            }

            var altars = graph.Rooms.Count(r => (r.Features & RoomFeatures.DeityAltar) != 0);
            var racePedestals = graph.Rooms.Count(r => (r.Features & RoomFeatures.RacePedestal) != 0);

            Assert.True(altars <= 1,
                $"Seed {seed} produced {altars} altars; entrance level allows at most 1");
            Assert.True(racePedestals <= 1,
                $"Seed {seed} produced {racePedestals} race pedestals; entrance level allows at most 1");
        }
    }

    [Fact]
    public void Never_assigns_curse_flag()
    {
        // EntranceLevelLayout sets CurseRoomProbability to zero; covered here so a future
        // regression that bumps the default is caught immediately.
        var layout = new EntranceLevelLayout();
        var parameters = layout.GetParameters(EntranceDifficulty);

        for (uint seed = 1; seed <= 100; seed++)
        {
            var graph = layout.BuildGraph(
                parameters, new SimpleRandom { Seed = seed }, new LevelGenerationStats(), new GenerationStats());
            if (graph is null)
            {
                continue;
            }

            Assert.DoesNotContain(graph.Rooms, r => (r.Features & RoomFeatures.Cursed) != 0);
        }
    }

    /// <summary>
    ///     <see cref="BranchData.Entrance" /> has <c>Difficulty = 0</c> and <c>Length = 1</c>,
    ///     so the only level produced by it sits at <see cref="GameLevel.Difficulty" /> = 1.
    /// </summary>
    private const byte EntranceDifficulty = 1;
}
