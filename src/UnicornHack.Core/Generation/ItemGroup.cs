using System;
using System.Collections.Generic;
using UnicornHack.Data;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class ItemGroup : ILoadable
{
    public ItemType Type
    {
        get;
        set;
    }

    public ItemComplexity Complexity
    {
        get;
        set;
    }

    public float Weight
    {
        get;
        set;
    }

    public IReadOnlyList<ItemGroup> SubGroups
    {
        get;
        set;
    }

    public string Name
    {
        get;
    } = "item groups";

    public static readonly SingleCSScriptLoader<ItemGroup> Loader =
        new SingleCSScriptLoader<ItemGroup>("Data", typeof(ItemGroupData));

    // TODO: Make more complex items more frequent on lower levels
    public IEnumerable<ItemGroup> GetGroups(Item item)
    {
        var queue = new Queue<ItemGroup>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.SubGroups == null || current.SubGroups.Count == 0)
            {
                yield return current;
                continue;
            }

            var subgroupMatched = false;
            foreach (var subGroup in current.SubGroups)
            {
                if ((subGroup.Type != ItemType.None && (item.Type & subGroup.Type) == 0) ||
                    (subGroup.Complexity != ItemComplexity.Normal && item.Complexity != subGroup.Complexity))
                {
                    continue;
                }

                if (subgroupMatched
                    && subGroup.Type == ItemType.None
                    && subGroup.Complexity == ItemComplexity.Normal)
                {
                    continue;
                }

                subgroupMatched = true;
                queue.Enqueue(subGroup);
            }

            if (!subgroupMatched)
            {
                throw new InvalidOperationException($"Couldn't find a subgroup of {current.Type} for {item.Name}");
            }
        }
    }
}
