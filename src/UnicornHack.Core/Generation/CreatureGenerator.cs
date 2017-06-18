using System;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class CreatureGenerator
    {
        public virtual byte ExpectedInitialCount { get; set; } = 6;

        public virtual void Fill(Level level)
        {
            var creaturesToPlace = level.GenerationRandom.NextBinomial(0.5f, ExpectedInitialCount * 2);
            var roomsToFill = level.Rooms.Count;
            var difficultyFraction = (float)(level.Difficulty - 1) / Level.MaxDifficulty;
            foreach (var room in level.GenerationRandom.WeightedOrder(level.Rooms.ToList(), r => r.InsidePoints.Count))
            {
                var creaturesPerRoom = (int)Math.Ceiling((float)creaturesToPlace / roomsToFill);
                for (var i = 0; i < creaturesPerRoom; i++)
                {
                    if (!level.GenerationRandom.TryPick(room.InsidePoints,
                        p => !level.Actors.Any(c => c.LevelX == p.X && c.LevelY == p.Y),
                        out var point))
                    {
                        goto NextRoom;
                    }

                    foreach (var creatureLevel in Sequence.GetAlternating(
                        level.GenerationRandom.NextBinomial(difficultyFraction, Creature.MaxLevel - 1),
                        0, Creature.MaxLevel - 1))
                    {
                        foreach (var creatureVariant in level.GenerationRandom.WeightedOrder(
                            Creature.Loader.GetAllValues((byte)(creatureLevel + 1)),
                            c => c.GetWeight(level)))
                        {
                            if (creatureVariant.Instantiate(level, point.X, point.Y) != null)
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
}