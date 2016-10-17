using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Weapon : Equipment
    {
        // For EF
        protected Weapon()
        {
        }

        public Weapon(ItemType type, Game game)
            : base(type, game)
        {
        }

        public byte Damage { get; set; }
    }
}