using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Knowledge;

namespace UnicornHack.Hubs
{
    public static class LogEntrySnapshot
    {
        public static List<object> Serialize(LogEntry entry, EntityState? state, SerializationContext context)
        {
            switch (state)
            {
                case null:
                    return new List<object>
                    {
                        entry.Id,
                        entry.Message,
                        entry.Tick
                    };
                case EntityState.Added:
                    return new List<object>
                    {
                        (int)state,
                        entry.Id,
                        entry.Message,
                        entry.Tick
                    };
                case EntityState.Deleted:
                    return new List<object>
                    {
                        (int)state,
                        entry.Id
                    };
                default:
                    var properties = new List<object>
                    {
                        (int)state,
                        entry.Id
                    };

                    var logEntry = context.DbContext.Entry(entry);
                    var i = 1;
                    var tick = logEntry.Property(nameof(LogEntry.Tick));
                    var message = logEntry.Property(nameof(LogEntry.Message));
                    if (message.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(entry.Message);
                    }

                    i++;
                    if (tick.IsModified)
                    {
                        properties.Add(i);
                        properties.Add(entry.Tick);
                    }

                    return properties.Count > 2 ? properties : null;
            }
        }
    }
}
