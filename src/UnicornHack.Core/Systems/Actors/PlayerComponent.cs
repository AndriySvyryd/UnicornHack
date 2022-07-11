using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors;

[Component(Id = (int)EntityComponent.Player)]
public class PlayerComponent : GameComponent, IKeepAliveComponent
{
    private string _properName = null!;
    private ActorAction? _nextAction;
    private int? _nextActionTarget;
    private int? _nextActionTarget2;
    private bool _queuedAction;
    private int? _nextActionTick;
    private int _maxLevel;
    private int _nextLevelXP;
    private int _skillPoints;
    private int _traitPoints;
    private int _mutationPoints;
    private int _handWeapons;
    private int _shortWeapons;
    private int _mediumWeapons;
    private int _longWeapons;
    private int _closeRangeWeapons;
    private int _shortRangeWeapons;
    private int _mediumRangeWeapons;
    private int _longRangeWeapons;
    private int _lightArmor;
    private int _heavyArmor;
    private int _artifice;

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
    } = new ObservableHashSet<PlayerCommand>();

    public string ProperName
    {
        get => _properName;
        set => SetWithNotify(value, ref _properName);
    }

    public ActorAction? NextAction
    {
        get => _nextAction;
        set => SetWithNotify(value, ref _nextAction);
    }

    public int? NextActionTarget
    {
        get => _nextActionTarget;
        set => SetWithNotify(value, ref _nextActionTarget);
    }

    public int? NextActionTarget2
    {
        get => _nextActionTarget2;
        set => SetWithNotify(value, ref _nextActionTarget2);
    }

    public bool QueuedAction
    {
        get => _queuedAction;
        set => SetWithNotify(value, ref _queuedAction);
    }

    public int? NextActionTick
    {
        get => _nextActionTick;
        set => SetWithNotify(value, ref _nextActionTick);
    }

    /// <summary>
    ///     The maximum level ever reached
    /// </summary>
    public int MaxLevel
    {
        get => _maxLevel;
        set => SetWithNotify(value, ref _maxLevel);
    }

    public int NextLevelXP
    {
        get => _nextLevelXP;
        set => SetWithNotify(value, ref _nextLevelXP);
    }

    public int SkillPoints
    {
        get => _skillPoints;
        set => SetWithNotify(value, ref _skillPoints);
    }

    public int TraitPoints
    {
        get => _traitPoints;
        set => SetWithNotify(value, ref _traitPoints);
    }

    public int MutationPoints
    {
        get => _mutationPoints;
        set => SetWithNotify(value, ref _mutationPoints);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int HandWeapons
    {
        get => _handWeapons;
        set => SetWithNotify(value, ref _handWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int ShortWeapons
    {
        get => _shortWeapons;
        set => SetWithNotify(value, ref _shortWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int MediumWeapons
    {
        get => _mediumWeapons;
        set => SetWithNotify(value, ref _mediumWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int LongWeapons
    {
        get => _longWeapons;
        set => SetWithNotify(value, ref _longWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int CloseRangeWeapons
    {
        get => _closeRangeWeapons;
        set => SetWithNotify(value, ref _closeRangeWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int ShortRangeWeapons
    {
        get => _shortRangeWeapons;
        set => SetWithNotify(value, ref _shortRangeWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int MediumRangeWeapons
    {
        get => _mediumRangeWeapons;
        set => SetWithNotify(value, ref _mediumRangeWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int LongRangeWeapons
    {
        get => _longRangeWeapons;
        set => SetWithNotify(value, ref _longRangeWeapons);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int LightArmor
    {
        get => _lightArmor;
        set => SetWithNotify(value, ref _lightArmor);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int HeavyArmor
    {
        get => _heavyArmor;
        set => SetWithNotify(value, ref _heavyArmor);
    }

    [Property(MinValue = 0, MaxValue = 3)]
    public int Artifice
    {
        get => _artifice;
        set => SetWithNotify(value, ref _artifice);
    }

    protected override void Clean()
    {
        _properName = default!;
        _nextAction = default;
        _nextActionTarget = default;
        _nextActionTarget2 = default;
        _queuedAction = default;
        _nextActionTick = default;
        _maxLevel = default;
        _nextLevelXP = default;
        _skillPoints = default;
        _traitPoints = default;
        _mutationPoints = default;
        _handWeapons = default;
        _shortWeapons = default;
        _mediumWeapons = default;
        _longWeapons = default;
        _closeRangeWeapons = default;
        _shortRangeWeapons = default;
        _mediumRangeWeapons = default;
        _longRangeWeapons = default;
        _lightArmor = default;
        _heavyArmor = default;
        _artifice = default;

        base.Clean();
    }
}
