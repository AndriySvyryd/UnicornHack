using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Position)]
public class PositionComponent : GameComponent
{
    private GameEntity? _roomEntity;
    private int _levelId;
    private byte? _cellX;
    private byte? _cellY;
    private Direction? _heading;

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

    public byte? CellX
    {
        get => _cellX;
        set => SetWithNotify(value, ref _cellX);
    }

    public byte? CellY
    {
        get => _cellY;
        set => SetWithNotify(value, ref _cellY);
    }

    /// <summary>
    ///     The cell within <see cref="RoomEntity" /> the entity occupies, or <c>null</c> when it is in the
    ///     room but not at a cell of its own, such as an item carried inside a container. Such
    ///     an entity keeps its <see cref="PositionComponent" /> (with <see cref="RoomId" /> tracking its
    ///     holder's room) but is excluded from the room's by-cell actor/item indexes.
    /// </summary>
    public Point? Cell
    {
        get => _cellX is { } x && _cellY is { } y ? new Point(x, y) : null;
        set
        {
            var newX = value?.X;
            var newY = value?.Y;
            var levelXSet = NotifyChanging(newX, ref _cellX, nameof(CellX), out var oldLevelX);
            var levelYSet = NotifyChanging(newY, ref _cellY, nameof(CellY), out var oldLevelY);

            if (Entity == null!)
            {
                return;
            }

            var anyChanges = false;
            var cellXChange = PropertyValueChange<byte?>.Empty;
            if (levelXSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(CellX));
                cellXChange = new PropertyValueChange<byte?>(this, nameof(CellX), oldLevelX, newX);
            }

            var cellYChange = PropertyValueChange<byte?>.Empty;
            if (levelYSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(CellY));
                cellYChange = new PropertyValueChange<byte?>(this, nameof(CellY), oldLevelY, newY);
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

    [Property<int>(MinValue = 0)]
    public int MovementDelay => Entity.ActiveEffects!.ResolveInt(nameof(MovementDelay));

    [Property<int>(MinValue = 0)]
    public int TurningDelay => Entity.ActiveEffects!.ResolveInt(nameof(TurningDelay));

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

        var cellXChange = PropertyValueChange<byte?>.Empty;
        if (cellXSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(CellX));
            cellXChange = new PropertyValueChange<byte?>(this, nameof(CellX), oldCellX, levelCell.X);
        }

        var cellYChange = PropertyValueChange<byte?>.Empty;
        if (cellYSet)
        {
            anyChanges = true;
            NotifyChanged(nameof(CellY));
            cellYChange = new PropertyValueChange<byte?>(this, nameof(CellY), oldCellY, levelCell.Y);
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
        Knowledge = default;

        base.Clean();
    }
}
