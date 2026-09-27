using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class SetAbilitySlotMessage : IMessage
{
    public const string Name = "SetAbilitySlot";

    public static SetAbilitySlotMessage Create(GameEntity actorEntity, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<SetAbilitySlotMessage>(Name);
        message.ActorEntity = actorEntity;
        return message;
    }

    private GameEntity? _abilityEntity;
    private GameEntity? _actorEntity;

    public GameEntity? AbilityEntity
    {
        get => _abilityEntity;
        set
        {
            _abilityEntity?.RemoveReference(this);
            _abilityEntity = value;
            _abilityEntity?.AddReference(this);
        }
    }

    public GameEntity ActorEntity
    {
        get => _actorEntity!;
        set
        {
            _actorEntity?.RemoveReference(this);
            _actorEntity = value;
            _actorEntity?.AddReference(this);
        }
    }

    public int? Slot
    {
        get;
        set;
    }

    string IMessage.MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public void Clean()
    {
        AbilityEntity = default;
        ActorEntity = default!;
        Slot = default;
    }
}
