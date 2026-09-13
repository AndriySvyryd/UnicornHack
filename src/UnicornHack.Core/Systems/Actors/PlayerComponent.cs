using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors;

[Component(Id = (int)EntityComponent.Player)]
public class PlayerComponent : ActorComponent, IKeepAliveComponent
{
    /// <summary>The highest rank any weapon or armor skill can be trained to.</summary>
    private bool _queuedAction;
    private PlayerOptions? _options;

    public PlayerComponent()
    {
        ComponentId = (int)EntityComponent.Player;
    }

    public ObservableHashSet<LogEntry> LogEntries
    {
        get;
    } = new();

    public ICollection<PlayerCommand> CommandHistory
    {
        get;
    } = new ObservableHashSet<PlayerCommand>(ReferenceEqualityComparer.Instance);

    public bool QueuedAction
    {
        get => _queuedAction;
        set => SetWithNotify(value, ref _queuedAction);
    }

    public PlayerOptions? PendingChoice
    {
        get => _options;
        set => SetWithNotify(value, ref _options);
    }

    protected override void Clean()
    {
        _queuedAction = default;
        _options = default;

        base.Clean();
    }
}
