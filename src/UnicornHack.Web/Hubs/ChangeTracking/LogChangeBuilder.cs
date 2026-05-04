using UnicornHack.Systems.Knowledge;

namespace UnicornHack.Hubs.ChangeTracking;

public class LogChangeBuilder
{
    private int _lastSentLogId;

    public bool HasChanges(GameEntity playerEntity)
        => playerEntity.Player!.LogEntries.Any(e => e.Id > _lastSentLogId);

    public void Initialize(GameEntity playerEntity)
    {
        var logEntries = playerEntity.Player!.LogEntries;
        if (logEntries.Count > 0)
        {
            _lastSentLogId = logEntries.Max(e => e.Id);
        }
    }

    public Dictionary<int, LogEntryChange>? GetSerializedLogEntries(GameEntity playerEntity)
    {
        var logEntries = new Dictionary<int, LogEntryChange>();
        foreach (var entry in playerEntity.Player!.LogEntries
                     .Where(e => e.Id > _lastSentLogId)
                     .OrderBy(e => e, LogEntry.Comparer))
        {
            logEntries[entry.Id] = SerializeLogEntry(entry);
        }

        foreach (var id in logEntries.Keys)
        {
            if (id > _lastSentLogId)
            {
                _lastSentLogId = id;
            }
        }

        return logEntries.Count > 0 ? logEntries : null;
    }

    public static LogEntryChange SerializeLogEntry(LogEntry entry)
        => new()
        {
            ChangedProperties = null,
            Message = entry.Message,
            Ticks = entry.Tick
        };
}
