using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Gold : Item
    {
        public Gold()
        {
        }

        public Gold(Game game)
            : base(game)
        {
        }

        public int Quantity { get; set; }

        public override IReadOnlyList<Item> Instantiate(IItemLocation location, int? quantity = null)
        {
            var item = (Gold)Instantiate(location.Game);
            item.Quantity = quantity ?? 1;

            if (!item.MoveTo(location))
            {
                item.Remove();
                return new List<Item>();
            }

            return new List<Item> {item};
        }

        public static Gold Get() => (Gold)Loader.Get("gold coin");

        protected override Item CreateInstance(Game game) => new Gold(game);

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

            var gold = (Gold)Get().Instantiate(Game);
            gold.Quantity = quantity;
            return gold.AddReference();
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<Gold>(
            GetPropertyConditions<Gold>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}