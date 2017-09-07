using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactProperty
    {
        public object[] Properties { get; set; }

        public static CompactProperty Serialize(Property property, GameDbContext context, GameServices services)
        {
            var properties = new object[2];
            var i = 0;
            properties[i++] = property.Name;
            properties[i] = services.Language.ToString(property);

            return new CompactProperty
            {
                Properties = properties
            };
        }
    }
}