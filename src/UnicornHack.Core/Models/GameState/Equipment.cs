using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Equipment : Item
    {
        // For EF
        protected Equipment()
        {
        }

        public Equipment(ItemType type, Game game)
            : base(type, game)
        {
        }

        public bool IsEquipped { get; set; }
        public EquipmentSlot Slot { get; set; }
        public int Enhancement { get; set; }
    }
}