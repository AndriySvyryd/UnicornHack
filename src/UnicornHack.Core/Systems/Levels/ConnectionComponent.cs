using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

[Component(Id = (int)EntityComponent.Connection)]
public class ConnectionComponent : GameComponent
{
    private GameEntity? _targetRoomEntity;
    private int _targetRoomId;
    private byte? _targetCellX;
    private byte? _targetCellY;
    private Directionality? _cardinality;
    private Direction _orientation;
    private bool _hidden;

    public ConnectionComponent()
    {
        ComponentId = (int)EntityComponent.Connection;
    }

    public int TargetRoomId
    {
        get => _targetRoomId;
        set
        {
            _targetRoomEntity = null;
            SetWithNotify(value, ref _targetRoomId);
        }
    }

    public GameEntity TargetRoomEntity
    {
        get
        {
            _targetRoomEntity ??= Entity.Manager.FindEntity(_targetRoomId);
            Debug.Assert(_targetRoomEntity != null, $"TargetRoomEntity {_targetRoomId} not loaded");
            return _targetRoomEntity!;
        }
        set
        {
            _targetRoomId = value.Id;
            _targetRoomEntity = value;
        }
    }

    public byte? TargetLevelX
    {
        get => _targetCellX;
        set => SetWithNotify(value, ref _targetCellX);
    }

    public byte? TargetLevelY
    {
        get => _targetCellY;
        set => SetWithNotify(value, ref _targetCellY);
    }

    public Point? TargetCell
    {
        get => _targetCellX == null || _targetCellY == null
            ? null
            : new Point(_targetCellX.Value, _targetCellY.Value);
        set
        {
            var levelXSet = NotifyChanging(value?.X, ref _targetCellX, nameof(TargetLevelX), out var oldLevelX);
            var levelYSet = NotifyChanging(value?.Y, ref _targetCellY, nameof(TargetLevelY), out var oldLevelY);

            var anyChanges = false;
            var levelXChange = PropertyValueChange<byte?>.Empty;
            if (levelXSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(TargetLevelX));
                levelXChange = new PropertyValueChange<byte?>(this, nameof(TargetLevelX), oldLevelX, value?.X);
            }

            var levelYChange = PropertyValueChange<byte?>.Empty;
            if (levelYSet)
            {
                anyChanges = true;
                NotifyChanged(nameof(TargetLevelY));
                levelYChange = new PropertyValueChange<byte?>(this, nameof(TargetLevelY), oldLevelY, value?.Y);
            }

            if (anyChanges)
            {
                Entity?.HandlePropertyValuesChanged(IPropertyValueChanges.Create(levelXChange, levelYChange));
            }
        }
    }

    public Directionality? Directionality
    {
        get => _cardinality;
        set => SetWithNotify(value, ref _cardinality);
    }

    /// <summary>
    ///     The cardinal direction the player faces when entering through this connection
    ///     (one of East, North, West, South). For inter-level connections this is
    ///     <see cref="Primitives.Direction.Down" /> or <see cref="Primitives.Direction.Up" />.
    /// </summary>
    public Direction Orientation
    {
        get => _orientation;
        set => SetWithNotify(value, ref _orientation);
    }

    /// <summary>
    ///     A hidden connection is one the player hasn't discovered yet -- it's part of the
    ///     room graph but isn't shown on the map until revealed (e.g. by clearing the source
    ///     room or activating a reveal boon).
    /// </summary>
    public bool Hidden
    {
        get => _hidden;
        set => SetWithNotify(value, ref _hidden);
    }

    protected override void Clean()
    {
        _targetRoomEntity = default;
        _targetRoomId = default;
        _targetCellX = default;
        _targetCellY = default;
        _cardinality = default;
        _orientation = default;
        _hidden = default;

        base.Clean();
    }
}
