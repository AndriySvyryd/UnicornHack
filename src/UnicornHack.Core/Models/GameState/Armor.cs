using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Armor : Equipment
    {
        // For EF
        protected Armor()
        {
        }

        public Armor(ItemType type, Game game)
            : base(type, game)
        {
        }

        public byte AC { get; set; }
    }
}