using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Equipment : Item
    {
        protected Equipment()
        {
        }

        public Equipment(ItemType type, Game game)
            : base(type, game)
        {
        }

        public Equipment(ItemType type, Level level, byte x, byte y)
            : base(type, level, x, y)
        {
        }

        public Equipment(ItemType type, Actor actor)
            : base(type, actor)
        {
        }

        public bool IsEquipped { get; set; }
        public EquipmentSlot Slot { get; set; }
        public int Enhancement { get; set; }
    }
}