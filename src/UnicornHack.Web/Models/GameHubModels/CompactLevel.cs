using System.Linq;
using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactLevel
    {
        public object[] Properties { get; set; }

        public static CompactLevel Serialize(Level level, GameDbContext context, GameServices services)
            => new CompactLevel
            {
                Properties = new object[]
                {
                    level.BranchName,
                    level.Depth,
                    level.Width,
                    level.Height,
                    level.Terrain,
                    level.WallNeighbours,
                    level.VisibleTerrain,
                    level.Actors.Select(a => CompactActor.Serialize(a, context, services)).ToArray(),
                    level.Items.Select(t => CompactItem.Serialize(t, context, services)).ToArray(),
                    level.Connections.Select(c => CompactConnection.Serialize(c, context, services)).ToArray()
                }
            };
    }
}