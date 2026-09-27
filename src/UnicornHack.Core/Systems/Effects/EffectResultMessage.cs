using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

public abstract class EffectResultMessage : IMessage
{
    private GameEntity? _activatorEntity;
    private GameEntity? _targetEntity;

    public GameEntity ActivatorEntity
    {
        get => _activatorEntity!;
        set
        {
            _activatorEntity?.RemoveReference(this);
            _activatorEntity = value;
            _activatorEntity?.AddReference(this);
        }
    }

    public GameEntity TargetEntity
    {
        get => _targetEntity!;
        set
        {
            _targetEntity?.RemoveReference(this);
            _targetEntity = value;
            _targetEntity?.AddReference(this);
        }
    }

    public Dictionary<EffectType, int> ResourceChanges
    {
        get;
    } = [];

    public bool AnyEffectApplied
    {
        get;
        set;
    }

    public abstract string MessageName
    {
        get;
        set;
    }

    public abstract void ReturnMessage(GameManager manager);

    public virtual void Clean()
    {
        ActivatorEntity = default!;
        TargetEntity = default!;
        ResourceChanges.Clear();
        AnyEffectApplied = default;
    }
}
