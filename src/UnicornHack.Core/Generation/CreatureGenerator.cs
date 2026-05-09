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
        var actorsCount = room.Actors.Count;
        var creaturesToPlace = room.Level.GenerationRandom.NextBinomial(0.5f, (ExpectedInitialCount - actorsCount) * 2);
        var roomsToFill = chunks.Count;
        var difficultyFraction = (float)(room.Level.Difficulty - 1) / LevelGenerator.MaxDifficulty;

        foreach (var chunk in room.Level.GenerationRandom.WeightedOrder(chunks, r => r.InsidePoints.Count))
        {
            var creaturesPerRoom = (int)Math.Ceiling((float)creaturesToPlace / roomsToFill);
            for (var i = 0; i < creaturesPerRoom; i++)
            {
                if (!room.Level.GenerationRandom.TryPick(
                        chunk.InsidePoints,
                        p => !room.Actors.ContainsKey(p),
                        out var point))
                {
                    goto NextRoom;
                }

                foreach (var creatureLevel in Sequence.GetAlternating(
                             room.Level.GenerationRandom.NextBinomial(difficultyFraction, Creature.MaxLevel - 1), 0,
                             Creature.MaxLevel - 1))
                {
                    foreach (var creatureVariant in room.Level.GenerationRandom.WeightedOrder(
                                 room.Level.Game.Services.Data.Creatures.GetAllValues((byte)(creatureLevel + 1)), c => c.GetWeight(room)))
                    {
                        if (creatureVariant.Instantiate(room, point) != null)
                        {
                            room.Level.GenerationStats.RecordCreatureInstance(
                                creatureVariant.Name, creatureVariant.GenerationFlags);
                            goto CreaturePlaced;
                        }
                    }

                    // TODO: Log no creatures of the chosen level
                }

                CreaturePlaced:
                creaturesToPlace--;
            }

            NextRoom:
            roomsToFill--;
        }
    }
}
