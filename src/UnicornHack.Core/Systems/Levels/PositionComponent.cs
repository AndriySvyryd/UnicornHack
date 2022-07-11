using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Position)]
public class PositionComponent : GameComponent, IKeepAliveComponent
{
    private GameEntity? _levelEntity;
    private int _levelId;
    private byte _levelX;
    private byte _levelY;
    private Direction? _heading;
    private int _movementDelay;
    private int _turningDelay;

    public PositionComponent()
    {
        ComponentId = (int)EntityComponent.Position;
    }

    public int LevelId
    {
        get => _levelId;
        set
        {
            _levelEntity = null;
            SetWithNotify(value, ref _levelId);
        }
    }

    public GameEntity LevelEntity
    {
        get => _levelEntity ??= Entity.Manager.FindEntity(_levelId)!;
        set
        {
            LevelId = value.Id;
            _levelEntity = value;
        }
    }

    public byte LevelX
    {
        get => _levelX;
        set => SetWithNotify(value, ref _levelX);
    }

    public byte LevelY
    {
        get => _levelY;
        set => SetWithNotify(value, ref _levelY);
    }

    public Point LevelCell
    {
        get => new(LevelX, LevelY);
        set
        {
            var levelXSet = NotifyChanging(value.X, ref _levelX, nameof(LevelX), out var oldLevelX);
            var levelYSet = NotifyChanging(value.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

            if (Entity == null!)
            {
                return;
            }

            var anyChanges = false;
            var levelXChange = PropertyValueChange<byte>.Empty;
            if (levelXSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(LevelX));
                levelXChange = new PropertyValueChange<byte>(this, nameof(LevelX), oldLevelX, value.X);
            }

            var levelYChange = PropertyValueChange<byte>.Empty;
            if (levelYSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(LevelY));
                levelYChange = new PropertyValueChange<byte>(this, nameof(LevelY), oldLevelY, value.Y);
            }

            if (anyChanges)
            {
                Entity.HandlePropertyValuesChanged(IPropertyValueChanges.Create(levelXChange, levelYChange));
            }
        }
    }

    public Direction? Heading
    {
        get => _heading;
        set => SetWithNotify(value, ref _heading);
    }

    [Property(MinValue = 0)]
    public int MovementDelay
    {
        get => _movementDelay;
        set => SetWithNotify(value, ref _movementDelay);
    }

    [Property(MinValue = 0)]
    public int TurningDelay
    {
        get => _turningDelay;
        set => SetWithNotify(value, ref _turningDelay);
    }

    public GameEntity? Knowledge
    {
        get;
        set;
    }

    /// <summary>
    ///     Sets <see cref="LevelId" /> and <see cref="LevelCell" /> atomically.
    /// </summary>
    public void SetLevelPosition(int levelId, Point levelCell)
    {
        var levelIdSet = NotifyChanging(levelId, ref _levelId, nameof(LevelId), out var oldLevelId);
        var levelXSet = NotifyChanging(levelCell.X, ref _levelX, nameof(LevelX), out var oldLevelX);
        var levelYSet = NotifyChanging(levelCell.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

        if (Entity == null!)
        {
            return;
        }

        var anyChanges = false;
        var levelIdChange = PropertyValueChange<int>.Empty;
        if (levelIdSet)
        {
            anyChanges = true;
            _levelEntity = null;
            NotifyChanged(nameof(LevelId));
            levelIdChange = new PropertyValueChange<int>(this, nameof(LevelId), oldLevelId, levelId);
        }

        var levelXChange = PropertyValueChange<byte>.Empty;
        if (levelXSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(LevelX));
            levelXChange = new PropertyValueChange<byte>(this, nameof(LevelX), oldLevelX, levelCell.X);
        }

        var levelYChange = PropertyValueChange<byte>.Empty;
        if (levelYSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(LevelY));
            levelYChange = new PropertyValueChange<byte>(this, nameof(LevelY), oldLevelY, levelCell.Y);
        }

        if (anyChanges)
        {
            Entity.HandlePropertyValuesChanged(IPropertyValueChanges.Create(levelIdChange, levelXChange,
                levelYChange));
        }
    }

    protected override void Clean()
    {
        _levelEntity = default;
        _levelId = default;
        _levelX = default;
        _levelY = default;
        _heading = default;
        _movementDelay = default;
        _turningDelay = default;
        Knowledge = default;

        base.Clean();
    }
}
