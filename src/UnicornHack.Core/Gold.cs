using System;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Gold : Item
    {
        public Gold()
        {
        }

        public Gold(Game game) : base(game)
        {
        }

        public int Quantity { get; set; }

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
            if (quantity > Quantity || quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            var gold = (Gold)GoldVariant.Get().Instantiate(Game);
            gold.Quantity = quantity;
            return gold.AddReference();
        }
    }
}