using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly GoldVariant GoldCoin = new GoldVariant
        {
            Name = "gold coin",
            Type = ItemType.Coin,
            Nameable = false,
            StackSize = 2147483647,
            ValuedProperties = new Dictionary<string, object> {{"weight", 0}}
        };
    }
}