using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class DeactivateAbilityMessage : IMessage
{
    public const string Name = "DeactivateAbility";

    public static DeactivateAbilityMessage Create(
        AbilityComponent ability, GameEntity activatorEntity, GameManager manager)
    {
        var message = manager.Queue.CreateMessage<DeactivateAbilityMessage>(Name);
        message.AbilityEntity = ability.Entity;
        message.Ability = ability;
        message.ActivatorEntity = activatorEntity;
        message.CaptureEffects(ability.Effects);
        return message;
    }

    private GameEntity? _activatorEntity;
    private GameEntity? _abilityEntity;
    private AbilityComponent? _ability;
    private readonly List<GameEntity> _effects = [];

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

    public IReadOnlyList<GameEntity> Effects => _effects;

    public AbilityComponent Ability
    {
        get => _ability!;
        private set
        {
            (_ability as IReferenceable)?.RemoveReference(this);
            _ability = value;
            (_ability as IReferenceable)?.AddReference(this);
        }
    }

    private void CaptureEffects(IEnumerable<GameEntity> effects)
    {
        foreach (var effect in effects)
        {
            effect.AddReference(this);
            _effects.Add(effect);
        }
    }

    string IMessage.MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public void Clean()
    {
        foreach (var effect in _effects)
        {
            effect.RemoveReference(this);
        }
        _effects.Clear();
        Ability = default!;
        ActivatorEntity = default!;
        AbilityEntity = default!;
    }
}
