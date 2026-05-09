using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class ItemGenerator
{
    public byte ExpectedInitialCount
    {
        get;
        set;
    } = 3;

    public virtual void Fill(RoomComponent room, IReadOnlyList<Chunk> chunks)
    {
        var itemsToPlace = room.Level.GenerationRandom.NextBinomial(
            0.5f,
            (ExpectedInitialCount - room.Items.Count) * 2);
        var roomsToFill = chunks.Count;
        var difficultyFraction = (float)(room.Level.Difficulty - 1) / LevelGenerator.MaxDifficulty;
        foreach (var chunk in room.Level.GenerationRandom.WeightedOrder(chunks, r => r.InsidePoints.Count))
        {
            var itemsPerRoom = (int)Math.Ceiling((float)itemsToPlace / roomsToFill);
            for (var i = 0; i < itemsPerRoom; i++)
            {
                if (!room.Level.GenerationRandom.TryPick(
                        chunk.InsidePoints,
                        p => room.Items.GetValueOrDefault(new Point(p.X, p.Y)) == null,
                        out var point))
                {
                    goto NextRoom;
                }

                var enumeratorsStack = new Stack<IEnumerator<ItemGroup>>();
                var data = room.Level.Game.Services.Data;
                var currentGroup = data.ItemGroups.Object;
                var itemPlaced = false;
                do
                {
                    if (currentGroup.SubGroups == null)
                    {
                        foreach (var itemVariant in room.Level.GenerationRandom.WeightedOrder(
                                     data.Items.GetAllValues(currentGroup), c => c.GetWeight(room)))
                        {
                            var quantity = currentGroup.Type == ItemType.Coin
                                ? room.Level.GenerationRandom.NextBinomial(difficultyFraction, 49) + 1
                                : 1;
                            if (itemVariant.Instantiate(room, point, quantity))
                            {
                                room.Level.GenerationStats.RecordItemInstance(
                                    itemVariant.Name, itemVariant.Type);
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
                        enumeratorsStack.Push(room.Level.GenerationRandom
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
