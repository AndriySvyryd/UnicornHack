using System;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class StackableItem : Item
    {
        public virtual short Quantity { get; set; }

        public static Item CreateItem(ItemType type, short quantity, byte x, byte y, Level level)
        {
            return CreateItem(CreateItem(type, quantity), x, y, level);
        }

        public static Item CreateItem(ItemType type, short quantity, Actor actor)
        {
            return CreateItem(CreateItem(type, quantity), actor);
        }

        private static Item CreateItem(ItemType type, short quantity)
        {
            StackableItem item;
            switch (type)
            {
                case ItemType.Gold:
                    item = new StackableItem {Name = "Gold"};
                    break;
                case ItemType.Food:
                    item = new StackableItem {Name = "Carrot"};
                    break;
                default:
                    throw new NotSupportedException($"Item type {type} not supported");
            }

            item.Type = type;
            item.Quantity = quantity;
            return item;
        }
    }
}