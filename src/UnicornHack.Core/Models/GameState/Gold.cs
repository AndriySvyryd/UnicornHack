using System;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Gold : Item
    {
        public Gold()
        {
        }

        public Gold(Game game, int quantity)
            : base(ItemVariant.Get("gold coin"), game)
        {
            Quantity = quantity;
        }

        public int Quantity { get; private set; }

        public override bool MoveTo(IItemLocation location)
        {
            var actor = location as Actor;
            if (actor == null)
            {
                return base.MoveTo(location);
            }

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

            return Create(Game, quantity).AddReference();
        }

        public static Gold Create(Game game, int quantity)
            => new Gold(game, quantity);
    }
}