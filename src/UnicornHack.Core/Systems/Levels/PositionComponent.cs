using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Position)]
public class PositionComponent : GameComponent, IKeepAliveComponent
{
    private GameEntity? _roomEntity;
    private int _levelId;
    private byte _cellX;
    private byte _cellY;
    private Direction? _heading;
    private int _movementDelay;
    private int _turningDelay;

    public PositionComponent()
    {
        ComponentId = (int)EntityComponent.Position;
    }

    public int RoomId
    {
        get => _levelId;
        set
        {
            _roomEntity = null;
            SetWithNotify(value, ref _levelId);
        }
    }

    public GameEntity RoomEntity
    {
        get
        {
            _roomEntity ??= Entity.Manager.FindEntity(_levelId);
            Debug.Assert(_roomEntity != null, $"RoomEntity {_levelId} not loaded");
            return _roomEntity!;
        }
        set
        {
            RoomId = value.Id;
            _roomEntity = value;
        }
    }

    public byte CellX
    {
        get => _cellX;
        set => SetWithNotify(value, ref _cellX);
    }

    public byte CellY
    {
        get => _cellY;
        set => SetWithNotify(value, ref _cellY);
    }

    public Point Cell
    {
        get => new(CellX, CellY);
        set
        {
            var levelXSet = NotifyChanging(value.X, ref _cellX, nameof(CellX), out var oldLevelX);
            var levelYSet = NotifyChanging(value.Y, ref _cellY, nameof(CellY), out var oldLevelY);

            if (Entity == null!)
            {
                return;
            }

            var anyChanges = false;
            var cellXChange = PropertyValueChange<byte>.Empty;
            if (levelXSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(CellX));
                cellXChange = new PropertyValueChange<byte>(this, nameof(CellX), oldLevelX, value.X);
            }

            var cellYChange = PropertyValueChange<byte>.Empty;
            if (levelYSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(CellY));
                cellYChange = new PropertyValueChange<byte>(this, nameof(CellY), oldLevelY, value.Y);
            }

            if (anyChanges)
            {
                Entity.HandlePropertyValuesChanged(IPropertyValueChanges.Create(cellXChange, cellYChange));
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
    ///     Sets <see cref="RoomId" /> and <see cref="Cell" /> atomically.
    /// </summary>
    public void SetRoomPosition(int levelId, Point levelCell)
    {
        var roomIdSet = NotifyChanging(levelId, ref _levelId, nameof(RoomId), out var oldRoomId);
        var cellXSet = NotifyChanging(levelCell.X, ref _cellX, nameof(CellX), out var oldCellX);
        var cellYSet = NotifyChanging(levelCell.Y, ref _cellY, nameof(CellY), out var oldCellY);

        if (Entity == null!)
        {
            return;
        }

        var anyChanges = false;
        var levelIdChange = PropertyValueChange<int>.Empty;
        if (roomIdSet)
        {
            anyChanges = true;
            _roomEntity = null;
            NotifyChanged(nameof(RoomId));
            levelIdChange = new PropertyValueChange<int>(this, nameof(RoomId), oldRoomId, levelId);
        }

        var cellXChange = PropertyValueChange<byte>.Empty;
        if (cellXSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(CellX));
            cellXChange = new PropertyValueChange<byte>(this, nameof(CellX), oldCellX, levelCell.X);
        }

        var cellYChange = PropertyValueChange<byte>.Empty;
        if (cellYSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(CellY));
            cellYChange = new PropertyValueChange<byte>(this, nameof(CellY), oldCellY, levelCell.Y);
        }

        if (anyChanges)
        {
            Entity.HandlePropertyValuesChanged(IPropertyValueChanges.Create(levelIdChange, cellXChange,
                cellYChange));
        }
    }

    protected override void Clean()
    {
        _roomEntity = default;
        _levelId = default;
        _cellX = default;
        _cellY = default;
        _heading = default;
        _movementDelay = default;
        _turningDelay = default;
        Knowledge = default;

        base.Clean();
    }
}
