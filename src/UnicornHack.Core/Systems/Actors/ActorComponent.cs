namespace UnicornHack.Systems.Actors;

/// <summary>
///     Common state shared by the actor components (<see cref="AIComponent" /> and
///     <see cref="PlayerComponent" />): identity, the next scheduled action and the timing used to
///     drive the turn loop and time-scaled effects such as resource changes over time.
/// </summary>
public abstract class ActorComponent : GameComponent
{
    private string? _properName;
    private ActorActionType? _nextAction;
    private int? _nextActionTarget;
    private int? _nextActionTarget2;
    private int? _nextActionTick;
    private int? _previousActionTick;

    public string? ProperName
    {
        get => _properName;
        set => SetWithNotify(value, ref _properName);
    }

    public ActorActionType? NextAction
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

    public int? NextActionTick
    {
        get => _nextActionTick;
        set => SetWithNotify(value, ref _nextActionTick);
    }

    /// <summary>
    ///     The tick at which this actor most recently took an action. Used to measure the elapsed
    ///     time since the last action when realizing time-scaled resource effects.
    /// </summary>
    public int? PreviousActionTick
    {
        get => _previousActionTick;
        set => SetWithNotify(value, ref _previousActionTick);
    }

    protected override void Clean()
    {
        _properName = default;
        _nextAction = default;
        _nextActionTarget = default;
        _nextActionTarget2 = default;
        _nextActionTick = default;
        _previousActionTick = default;

        base.Clean();
    }
}
