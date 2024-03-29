using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

public class ApplyEffectMessage : IMessage
{
    public const string Name = "ApplyEffect";

    public static ApplyEffectMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ApplyEffectMessage>(Name);

    private GameEntity? _activatorEntity;
    private GameEntity? _effectEntity;
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

    public GameEntity EffectEntity
    {
        get => _effectEntity!;
        set
        {
            _effectEntity?.RemoveReference(this);
            _effectEntity = value;
            _effectEntity?.AddReference(this);
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

    public Point? TargetCell
    {
        get;
        set;
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        ActivatorEntity = default!;
        EffectEntity = default!;
        TargetEntity = default!;
        TargetCell = default;
    }
}
