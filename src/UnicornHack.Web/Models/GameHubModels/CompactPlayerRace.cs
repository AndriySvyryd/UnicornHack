using UnicornHack.Data;
using UnicornHack.Effects;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactPlayerRace
    {
        public object[] Properties { get; set; }

        public static CompactPlayerRace Serialize(ChangedRace race, GameDbContext context, GameServices services)
            => new CompactPlayerRace
            {
                Properties = new object[]
                {
                    race.Name,
                    race.XPLevel,
                    race.IsLearning
                }
            };
    }
}