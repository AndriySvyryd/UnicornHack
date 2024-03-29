﻿using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public class CreatureGenerator
{
    public byte ExpectedInitialCount
    {
        get;
        set;
    } = 8;

    public virtual void Fill(LevelComponent level, IReadOnlyList<Room> rooms)
    {
        var actorsCount = level.Actors.Count;
        var creaturesToPlace = level.GenerationRandom.NextBinomial(0.5f, (ExpectedInitialCount - actorsCount) * 2);
        var roomsToFill = rooms.Count;
        var difficultyFraction = (float)(level.Difficulty - 1) / LevelGenerator.MaxDifficulty;

        foreach (var room in level.GenerationRandom.WeightedOrder(rooms, r => r.InsidePoints.Count))
        {
            var creaturesPerRoom = (int)Math.Ceiling((float)creaturesToPlace / roomsToFill);
            for (var i = 0; i < creaturesPerRoom; i++)
            {
                if (!level.GenerationRandom.TryPick(
                        room.InsidePoints,
                        p => !level.Actors.ContainsKey(p),
                        out var point))
                {
                    goto NextRoom;
                }

                foreach (var creatureLevel in Sequence.GetAlternating(
                             level.GenerationRandom.NextBinomial(difficultyFraction, Creature.MaxLevel - 1), 0,
                             Creature.MaxLevel - 1))
                {
                    foreach (var creatureVariant in level.GenerationRandom.WeightedOrder(
                                 Creature.Loader.GetAllValues((byte)(creatureLevel + 1)), c => c.GetWeight(level)))
                    {
                        if (creatureVariant.Instantiate(level, point) != null)
                        {
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
