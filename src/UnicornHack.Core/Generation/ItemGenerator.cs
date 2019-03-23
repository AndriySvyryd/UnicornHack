using System;
using System.Collections.Generic;
using UnicornHack.Generation.Map;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation
{
    public class ItemGenerator
    {
        public byte ExpectedInitialCount { get; set; } = 3;

        public virtual void Fill(LevelComponent level, List<Room> rooms)
        {
            var manager = level.Entity.Manager;
            var itemsToPlace = level.GenerationRandom.NextBinomial(0.5f,
                (ExpectedInitialCount - manager.LevelItemsToLevelRelationship[level.EntityId].Count) * 2);
            var roomsToFill = rooms.Count;
            var difficultyFraction = (float)(level.Difficulty - 1) / LevelGenerator.MaxDifficulty;
            foreach (var room in level.GenerationRandom.WeightedOrder(rooms, r => r.InsidePoints.Count))
            {
                var itemsPerRoom = (int)Math.Ceiling((float)itemsToPlace / roomsToFill);
                for (var i = 0; i < itemsPerRoom; i++)
                {
                    if (!level.GenerationRandom.TryPick(
                        room.InsidePoints,
                        p => manager.LevelItemsToLevelCellIndex[(level.EntityId, p.X, p.Y)] == null,
                        out var point))
                    {
                        goto NextRoom;
                    }

                    var enumeratorsStack = new Stack<IEnumerator<ItemGroup>>();
                    var currentGroup = ItemGroup.Loader.Object;
                    var itemPlaced = false;
                    do
                    {
                        if (currentGroup.SubGroups == null)
                        {
                            foreach (var itemVariant in level.GenerationRandom.WeightedOrder(
                                Item.Loader.GetAllValues(currentGroup), c => c.GetWeight(level)))
                            {
                                var quantity = currentGroup.Type == ItemType.Coin
                                    ? level.GenerationRandom.NextBinomial(difficultyFraction, 49) + 1
                                    : 1;
                                if (itemVariant.Instantiate(level, point, quantity))
                                {
                                    itemPlaced = true;
                                    break;
                                }
                            }

                            if (itemPlaced)
                            {
                                break;
                            }

                            // TODO: Log no item of the chosen subgroup
                        }
                        else
                        {
                            enumeratorsStack.Push(level.GenerationRandom
                                .WeightedOrder(currentGroup.SubGroups, g => g.Weight).GetEnumerator());
                        }

                        currentGroup = null;
                        while (enumeratorsStack.Count > 0)
                        {
                            var subgroups = enumeratorsStack.Peek();
                            if (subgroups.MoveNext())
                            {
                                currentGroup = subgroups.Current;
                                break;
                            }

                            enumeratorsStack.Pop();
                            subgroups.Dispose();
                        }
                    } while (currentGroup != null);

                    if (!itemPlaced)
                    {
                        throw new InvalidOperationException("No item could be placed");
                    }

                    while (enumeratorsStack.Count > 0)
                    {
                        var subgroups = enumeratorsStack.Pop();
                        subgroups.Dispose();
                    }

                    itemsToPlace--;
                }

                NextRoom:
                roomsToFill--;
            }
        }
    }
}
