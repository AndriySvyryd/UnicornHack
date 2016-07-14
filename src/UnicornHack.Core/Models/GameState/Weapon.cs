using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Weapon : Equipment
    {
        protected Weapon()
        {
        }

        public Weapon(ItemType type, Game game)
            : base(type, game)
        {
        }

        public Weapon(ItemType type, Level level, byte x, byte y)
            : base(type, level, x, y)
        {
        }

        public Weapon(ItemType type, Actor actor)
            : base(type, actor)
        {
        }

        public byte Damage { get; set; }
    }
}