using UnicornHack.Utils;

namespace UnicornHack.Systems.Actors;

public class PlayerCommand : NotificationEntity
{
    private int _gameId;
    private int _id;
    private int _playerId;
    private int _tick;
    private ActorAction _action;
    private int? _target;
    private int? _target2;

    public int GameId
    {
        get => _gameId;
        set => SetWithNotify(value, ref _gameId);
    }

    public int Id
    {
        get => _id;
        set => SetWithNotify(value, ref _id);
    }

    public int PlayerId
    {
        get => _playerId;
        set => SetWithNotify(value, ref _playerId);
    }

    public int Tick
    {
        get => _tick;
        set => SetWithNotify(value, ref _tick);
    }

    public ActorAction Action
    {
        get => _action;
        set => SetWithNotify(value, ref _action);
    }

    public int? Target
    {
        get => _target;
        set => SetWithNotify(value, ref _target);
    }

    public int? Target2
    {
        get => _target2;
        set => SetWithNotify(value, ref _target2);
    }
}
