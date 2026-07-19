using System.Collections.Immutable;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilityActivatedMessage : IMessage
{
    public const string Name = "AbilityActivated";

    public static AbilityActivatedMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<AbilityActivatedMessage>(Name);

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

    public ActivationType Trigger
    {
        get;
        set;
    }

    /// <summary>
    ///     The effect entities to apply. Unlike the entity references on this message, these
    ///     are intentionally <em>not</em> reference-counted: the list is shared by reference
    ///     across <see cref="Clone" />d messages and is built up immutably, so per-message
    ///     <see cref="Clean" /> can't own the elements' lifetimes. The effects are kept alive
    ///     by their containing ability (via <c>EffectsToContainingAbilityRelationship</c>) only
    ///     while that ability exists. Because this message is enqueued and processed later, a
    ///     containing ability — and thus its effect entities — may be removed in the interim
    ///     (e.g. a consumed or shattered item). Consumers must therefore check each entry is
    ///     still alive (<c>effectEntity.Effect is { }</c>) before using it.
    /// </summary>
    public ImmutableList<GameEntity> EffectsToApply
    {
        get;
        set;
    } = null!;

    public string? ActivationError
    {
        get;
        set;
    }

    public ApplicationOutcome Outcome
    {
        get;
        set;
    }

    public AbilityActivatedMessage Clone(GameManager manager)
    {
        var abilityActivatedMessage = manager.CreateMessage<AbilityActivatedMessage>(
            ((IMessage)this).MessageName);
        abilityActivatedMessage.ActivatorEntity = ActivatorEntity;
        abilityActivatedMessage.AbilityEntity = AbilityEntity;
        abilityActivatedMessage.TargetEntity = TargetEntity;
        abilityActivatedMessage.TargetCell = TargetCell;
        abilityActivatedMessage.Trigger = Trigger;
        abilityActivatedMessage.EffectsToApply = EffectsToApply;
        abilityActivatedMessage.ActivationError = ActivationError;
        return abilityActivatedMessage;
    }

    string IMessage.MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public void Clean()
    {
        ActivatorEntity = default!;
        AbilityEntity = default!;
        TargetEntity = default;
        TargetCell = default;
        Trigger = default;
        EffectsToApply = default!;
        ActivationError = default;
        Outcome = default;
    }
}
