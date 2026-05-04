namespace UnicornHack.Systems.Knowledge;

[Component(Id = (int)EntityComponent.Knowledge)]
public class KnowledgeComponent : GameComponent
{
    private GameEntity? _knownEntity;
    private int _knownEntityId;
    private SenseType _sensedType;
    private bool _isIdentified;

    public KnowledgeComponent()
    {
        ComponentId = (int)EntityComponent.Knowledge;
    }

    public int KnownEntityId
    {
        get => _knownEntityId;
        set
        {
            SetWithNotify(value, ref _knownEntityId);
            _knownEntity = null;
        }
    }

    public SenseType SensedType
    {
        get => _sensedType;
        set => SetWithNotify(value, ref _sensedType);
    }

    public bool IsIdentified
    {
        get => _isIdentified;
        set => SetWithNotify(value, ref _isIdentified);
    }

    // Unmapped properties

    public GameEntity KnownEntity
    {
        get => _knownEntity ??= Entity.Manager.FindEntity(_knownEntityId)!;
        set
        {
            KnownEntityId = value.Id;
            _knownEntity = value;
        }
    }

    protected override void Clean()
    {
        _knownEntity = default;
        _knownEntityId = default;
        _sensedType = default;
        _isIdentified = default;

        base.Clean();
    }
}
