using System.Collections.Generic;

namespace UnicornHack
{
    public interface IItemLocation
    {
        Game Game { get; }
        IEnumerable<Item> Items { get; }
        bool TryAdd(Item item);
        bool CanAdd(Item item);
    }
}