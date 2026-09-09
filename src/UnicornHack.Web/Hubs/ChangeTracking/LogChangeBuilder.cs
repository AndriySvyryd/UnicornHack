using System.Collections.Specialized;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Knowledge;

namespace UnicornHack.Hubs.ChangeTracking;

public class LogChangeBuilder
{
    private readonly List<LogEntry> _pendingEntries = [];
    private readonly PlayerComponent _player;

    public LogChangeBuilder(GameEntity playerEntity)
    {
        _player = playerEntity.Player!;
        _player.LogEntries.CollectionChanged += OnLogEntriesChanged;
    }

    public bool HasChanges => _pendingEntries.Count != 0;

    public void Reset() => _pendingEntries.Clear();

    public void Unregister()
        => _player.LogEntries.CollectionChanged -= OnLogEntriesChanged;

    public Dictionary<int, LogEntryChange>? GetSerializedLogEntries()
    {
        if (_pendingEntries.Count == 0)
        {
            return null;
        }

        _pendingEntries.Sort(LogEntry.Comparer);
        var serialized = new Dictionary<int, LogEntryChange>(_pendingEntries.Count);
        foreach (var entry in _pendingEntries)
        {
            serialized.Add(entry.Id, SerializeLogEntry(entry));
        }

        _pendingEntries.Clear();
        return serialized;
    }

    public static LogEntryChange SerializeLogEntry(LogEntry entry)
        => new()
        {
            ChangedProperties = null,
            Message = entry.Message,
            Ticks = entry.Tick
        };

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.NewItems == null)
        {
            return;
        }

        foreach (LogEntry entry in args.NewItems)
        {
            _pendingEntries.Add(entry);
        }
    }
}
