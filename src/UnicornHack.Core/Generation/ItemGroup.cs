using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Data;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class ItemGroup
    {
        public ItemType Type { get; set; }
        public float Weight { get; set; }
        public IReadOnlyList<ItemGroup> SubGroups { get; set; }

        // Loader isn't declared on ItemGroupData to allow it to be loaded lazily
        public static readonly SingleCSScriptLoader<ItemGroup> Loader =
            new SingleCSScriptLoader<ItemGroup>("Data", "item groups", typeof(ItemGroupData));

        public IEnumerable<ItemGroup> GetGroups(ItemVariant item)
        {
            var queue = new Queue<ItemGroup>();
            queue.Enqueue(this);
            var itemTypes = item.Type;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.SubGroups == null
                    || current.SubGroups.Count == 0)
                {
                    yield return current;
                    continue;
                }

                var subgroupMatched = false;
                foreach (var subGroup in current.SubGroups.Where(g => g.Type != ItemType.None))
                {
                    if (itemTypes.HasFlag(subGroup.Type))
                    {
                        subgroupMatched = true;
                        queue.Enqueue(subGroup);
                    }
                }

                if (!subgroupMatched)
                {
                    var defaultGroup = current.SubGroups.FirstOrDefault(g => g.Type == ItemType.None);
                    if (defaultGroup == null)
                    {
                        throw new InvalidOperationException($"Couldn't find a subgroup of {current.Type} for {item.Name}");
                    }
                    queue.Enqueue(defaultGroup);
                }
            }
        }
    }
}