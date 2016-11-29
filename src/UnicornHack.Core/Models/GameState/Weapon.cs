using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Weapon : Equipment
    {
        // For EF
        protected Weapon()
        {
        }

        public Weapon(ItemVariant variant, Game game)
            : base(variant, game)
        {
        }

        public byte Damage { get; set; }
    }
}