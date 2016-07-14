using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class StackableItem : Item
    {
        protected StackableItem()
        {
        }

        public StackableItem(ItemType type, Level level, byte x, byte y, short quantity)
            : base(type, level, x, y)
        {
            Quantity = quantity;
        }

        public StackableItem(ItemType type, Actor actor, short quantity)
            : base(type, actor)
        {
            Quantity = quantity;
        }

        public virtual short Quantity { get; set; }
    }
}