using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Generation
{
    public class ItemGroup
    {
        public ItemType Type { get; set; }
        public float Weight { get; set; }
        public IReadOnlyList<ItemGroup> SubGroups { get; set; }

        public IEnumerable<ItemGroup> GetGroups(Item item)
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