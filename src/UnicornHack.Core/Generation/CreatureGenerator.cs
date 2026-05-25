using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public class CreatureGenerator
{
    public byte ExpectedInitialCount
    {
        get;
        set;
    } = 8;

    public virtual void Fill(RoomComponent room, IReadOnlyList<Chunk> chunks)
    {
        // Authored spawn points bypass the random budget; we count them up front so the
        // random pass only fills the remaining headroom toward ExpectedInitialCount.
        var actorsCount = room.Actors.Count;
        var creaturesToPlace = room.Level.GenerationRandom.NextBinomial(0.5f, (ExpectedInitialCount - actorsCount) * 2);
        var difficultyFraction = (float)(room.Level.Difficulty - 1) / LevelGenerator.MaxDifficulty;

        // Authored spawn points always spawn if the tile is free, regardless of budget or
        // chunk kind — so an inbound-connection fragment can still place a guardian by
        // marking the tile in its payload.
        foreach (var chunk in chunks)
        {
            for (var j = 0; j < chunk.CreaturePoints.Count; j++)
            {
                var point = chunk.CreaturePoints[j];
                if (!room.Actors.ContainsKey(point))
                {
                    TrySpawnCreatureAt(room, point, difficultyFraction);
                }
            }
        }

        // Random body: weight ConnectingMapFragment chunks at zero so the weighted iterator
        // never picks them, which prevents random creature placement on chunks that host an
        // inbound level connection unless the fragment placed an authored spawn point above.
        var roomsToFill = 0;
        foreach (var chunk in chunks)
        {
            if (chunk.Fragment is not ConnectingMapFragment)
            {
                roomsToFill++;
            }
        }

        foreach (var chunk in room.Level.GenerationRandom.WeightedOrder(
                     chunks,
                     c => c.Fragment is ConnectingMapFragment ? 0f : c.InsidePoints.Count))
        {
            var creaturesPerRoom = (int)Math.Ceiling((float)creaturesToPlace / roomsToFill);
            for (var i = 0; i < creaturesPerRoom; i++)
            {
                if (!room.Level.GenerationRandom.TryPick(
                        chunk.InsidePoints,
                        p => !room.Actors.ContainsKey(p),
                        out var point))
                {
                    break;
                }

                if (TrySpawnCreatureAt(room, point, difficultyFraction))
                {
                    creaturesToPlace--;
                }

                // TODO: Log no creatures of the chosen level
            }

            roomsToFill--;
        }
    }

    private static bool TrySpawnCreatureAt(RoomComponent room, Point point, float difficultyFraction)
    {
        foreach (var creatureLevel in Sequence.GetAlternating(
                     room.Level.GenerationRandom.NextBinomial(difficultyFraction, Creature.MaxLevel - 1), 0,
                     Creature.MaxLevel - 1))
        {
            foreach (var creatureVariant in room.Level.GenerationRandom.WeightedOrder(
                         room.Level.Game.Services.Data.Creatures.GetAllValues((byte)(creatureLevel + 1)),
                         c => c.GetWeight(room)))
            {
                if (creatureVariant.Instantiate(room, point) != null)
                {
                    room.Level.GenerationStats.RecordCreatureInstance(
                        creatureVariant.Name, creatureVariant.GenerationFlags);
                    return true;
                }
            }
        }

        return false;
    }
}
