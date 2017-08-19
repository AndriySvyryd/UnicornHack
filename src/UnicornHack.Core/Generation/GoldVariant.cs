using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation
{
    public class GoldVariant : ItemVariant
    {
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

        protected override Item CreateInstance(Game game) => new Gold(game);

        public static GoldVariant Get() => (GoldVariant)Loader.Get("gold coin");

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<GoldVariant>(GetPropertyConditions<GoldVariant>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}