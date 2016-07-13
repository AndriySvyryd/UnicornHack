using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Equipment : Item
    {
        public bool IsEquipped { get; set; }
        public EquipmentSlot Slot { get; set; }
        public int Enhancement { get; set; }
    }
}