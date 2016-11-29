using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Armor : Equipment
    {
        // For EF
        protected Armor()
        {
        }

        public Armor(ItemVariant variant, Game game)
            : base(variant, game)
        {
        }

        public byte AC { get; set; }
    }
}