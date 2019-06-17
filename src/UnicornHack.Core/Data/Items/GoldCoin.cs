using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item GoldCoin = new Item
        {
            Name = "gold coin", Type = ItemType.Coin, Material = Material.Gold, Weight = 0, StackSize = 2147483647,
            Countable = true, Nameable = false
        };
    }
}
