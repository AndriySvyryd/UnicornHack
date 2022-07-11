using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class ActivateAbilityMessage : IMessage
{
    public const string Name = "ActivateAbility";

    public static ActivateAbilityMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ActivateAbilityMessage>(Name);

    public static ActivateAbilityMessage Create(GameEntity abilityEntity, GameEntity activatorEntity,
        GameEntity targetEntity)
    {
        var activationMessage = abilityEntity.Manager.Queue.CreateMessage<ActivateAbilityMessage>(Name);

        activationMessage.AbilityEntity = abilityEntity;
        activationMessage.ActivatorEntity = activatorEntity;
        activationMessage.TargetEntity = targetEntity;

        return activationMessage;
    }

    private GameEntity? _activatorEntity;
    private GameEntity? _abilityEntity;
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

    public GameEntity AbilityEntity
    {
        get => _abilityEntity!;
        set
        {
            _abilityEntity?.RemoveReference(this);
            _abilityEntity = value;
            _abilityEntity?.AddReference(this);
        }
    }

    public GameEntity? TargetEntity
    {
        get => _targetEntity;
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

    public ActivateAbilityMessage Clone(ActivateAbilityMessage message)
    {
        var manager = message.ActivatorEntity.Manager;

        var abilityMessage = Create(manager);
        abilityMessage.AbilityEntity = message.AbilityEntity;
        abilityMessage.ActivatorEntity = message.ActivatorEntity;
        abilityMessage.TargetEntity = message.TargetEntity;
        abilityMessage.TargetCell = message.TargetCell;

        return abilityMessage;
    }

    string IMessage.MessageName
    {
        get;
        set;
    } = null!;

    public void Clean()
    {
        ActivatorEntity = default!;
        AbilityEntity = default!;
        TargetEntity = default!;
        TargetCell = default;
    }
}
