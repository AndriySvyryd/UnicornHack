using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Armor : Equipment
    {
        protected Armor()
        {
        }

        public Armor(ItemType type, Game game)
            : base(type, game)
        {
        }

        public Armor(ItemType type, Level level, byte x, byte y)
            : base(type, level, x, y)
        {
        }

        public Armor(ItemType type, Actor actor)
            : base(type, actor)
        {
        }

        public byte AC { get; set; }
    }
}