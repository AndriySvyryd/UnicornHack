using System;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Gold : Item
    {
        // For EF
        protected Gold()
        {
        }

        public Gold(int quantity, Game game)
            : base(ItemType.Gold, game)
        {
            Quantity = quantity;
        }

        public int Quantity { get; private set; }

        public override bool MoveTo(Actor actor)
        {
            Remove();
            actor.Gold += Quantity;

            return true;
        }

        public override TransientReference<Item> Split(int quantity)
        {
            if (quantity > Quantity
                || quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            return new Gold(quantity, Game).AddReference();
        }
    }
}