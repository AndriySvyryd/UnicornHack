using System.Collections.Generic;

namespace UnicornHack.Models.GameState
{
    public interface IItemLocation
    {
        Game Game { get; }
        IEnumerable<Item> Items { get; }
        bool TryAdd(Item item);
        bool CanAdd(Item item);
    }
}