using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Knowledge;

namespace UnicornHack.Hubs;

public static class LogEntrySnapshot
{
    public static List<object?>? Serialize(LogEntry entry, EntityState? state, SerializationContext context)
    {
        switch (state)
        {
            case null:
            case EntityState.Added:
                return [null, entry.Message, entry.Tick];
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
                var logEntry = context.DbContext.Entry(entry);
                if (logEntry.State == EntityState.Unchanged)
                {
                    return null;
                }

                var i = 0;
                var setValues = new bool[3];
                setValues[i++] = true;
                List<object?> properties = [null];

                var message = logEntry.Property(nameof(LogEntry.Message));
                if (message.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(entry.Message);
                }
                else
                {
                    setValues[i++] = false;
                }

                var tick = logEntry.Property(nameof(LogEntry.Tick));
                if (tick.IsModified)
                {
                    setValues[i++] = true;
                    properties.Add(entry.Tick);
                }
                else
                {
                    setValues[i++] = false;
                }

                Debug.Assert(i == 3);
                if (properties.Count == 1)
                {
                    return null;
                }

                properties[0] = new BitArray(setValues);
                return properties;
        }
    }
}
