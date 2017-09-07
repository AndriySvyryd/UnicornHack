using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactConnection
    {
        public object[] Properties { get; set; }

        public static CompactConnection Serialize(Connection connection, GameDbContext context, GameServices services)
            => new CompactConnection
            {
                Properties = new object[]
                {
                    connection.LevelX,
                    connection.LevelY,
                    connection.TargetLevelDepth > connection.LevelDepth
                }
            };
    }
}
