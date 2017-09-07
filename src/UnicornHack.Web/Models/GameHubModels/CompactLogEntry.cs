using UnicornHack.Data;
using UnicornHack.Services;

namespace UnicornHack.Models.GameHubModels
{
    public class CompactLogEntry
    {
        public object[] Properties { get; set; }

        public static CompactLogEntry Serialize(LogEntry entry, GameDbContext context, GameServices services)
            => new CompactLogEntry
            {
                Properties = new object[]
                {
                    entry.Id,
                    $"{entry.Tick / 100f:0000.00}: {entry.Message}"
                }
            };
    }
}