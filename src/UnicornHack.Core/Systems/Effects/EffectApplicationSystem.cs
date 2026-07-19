using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

/// <summary>
///     Applies the effects an ability delivers to a target and maintains the lingering ones over time.
///     The model is uniform:
///     <list type="number">
///         <item>
///             An effect either mutates the target immediately (damage, heal, drain, move, equip, gain XP, a
///             structural race/ability add or remove) or banks a lingering <see cref="ActivePropertyEffect" />
///             on the target's <see cref="ActiveEffectsComponent" /> - or both. Instant effects only mutate;
///             non-instant ones bank an instance. There are no separate applied-effect entities.
///         </item>
///         <item>
///             Every lingering contribution is one <see cref="ActivePropertyEffect" /> in the target's single
///             <see cref="ActiveEffectsComponent.PropertyEffects" /> list, told apart by its fields: a property
///             modifier (adds <c>MagnitudePrimary</c> to <c>TargetName</c> via a
///             <see cref="ValueCombinationFunction" />), a build-up accumulator (<c>BuildUp</c> function,
///             banking toward a threshold), a doom/overcharge/lycanthropy counter, a resource-over-time rate, a
///             reservation, or a polymorph marker. Resolved dynamic properties are computed on read by folding
///             these instances - the system never writes a backing field.
///         </item>
///         <item>
///             An instance carries its own lifetime: a tick or XP expiry (surfaced through
///             <see cref="ActiveEffectsComponent.NextEffectTick" />), or a source link that drops it when the
///             sustaining ability deactivates. Per-turn evolution (resource rates, build-up fade, counters)
///             is realized at the actor's turn start, scaled by the elapsed time.
///         </item>
///         <item>
///             The target's defenses (ward, grease, doom immunity) turn a lingering status aside uniformly,
///             before it takes hold, on every path.
///         </item>
///         <item>
///             Runtime application emits focused attack or item-activation result messages; previews expose
///             the complete authored effect list directly.
///         </item>
///     </list>
/// </summary>
public partial class EffectApplicationSystem :
    IGameSystem<AbilityActivatedMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<TraveledMessage>
{
    public const string InnateAbilityName = "innate";

    public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager manager)
    {
        ApplyEffects(message, pretend: false);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (!message.Successful
            || message.Entity.Position?.RoomEntity is not { } currentRoomEntity
            || message.InitialLevel == currentRoomEntity)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var currentRoom = currentRoomEntity.Room!;
        var levelChanged = message.InitialBranchName != currentRoom.BranchName
                   || message.InitialDepth != currentRoom.Depth;
        ExpireTransitionEffects(message.Entity, levelChanged, manager);

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        // An entity joins AffectableEntities exactly when it gains its ActiveEffectsComponent - the
        // component that now defines an affectable entity - so it already owns one here; seed the resolved
        // properties of the components present at this point. Every actor and item creates its
        // ActiveEffectsComponent when instantiated, after its other components.
        InitializeProperties(message.Entity);

        return MessageProcessingResult.ContinueProcessing;
    }

    private void ApplyEffects(AbilityActivatedMessage message, bool pretend)
    {
        if (message.TargetEntity == null
            || message.AbilityEntity.Ability is not { } ability)
        {
            return;
        }

        var manager = message.ActivatorEntity.Manager;
        var successfulApplication = message.Outcome == ApplicationOutcome.Success;
        EffectResultMessage? result = ability.Action is not (AbilityAction.Default or AbilityAction.Modifier)
            ? AttackResultMessage.Create(manager)
            : null;
        try
        {
            foreach (var effectEntity in message.EffectsToApply)
            {
                if (effectEntity.Effect is not { ContainingAbilityId: not null } effect
                    || effect.ContainingAbility?.Ability == null)
                {
                    continue;
                }

                var metadata = Effect.GetMetadata(effect.EffectType);
                if (metadata.ResultMessageSelector is { } selector
                    && selector(effect, message.ActivatorEntity, manager) is { } selectedResult)
                {
                    if (result == null
                        || (result is AttackResultMessage
                            && selectedResult is ItemActivationResultMessage))
                    {
                        result?.ReturnMessage(manager);
                        result = selectedResult;
                    }
                    else
                    {
                        selectedResult.ReturnMessage(manager);
                    }
                }
            }

            ApplyEffectsCore(message, result, successfulApplication, pretend);
            EnqueueResult(result, message, ability, successfulApplication, manager);
            result = null;
        }
        catch
        {
            result?.ReturnMessage(manager);
            throw;
        }
    }

    private void ApplyEffectsCore(
        AbilityActivatedMessage message, EffectResultMessage? result,
        bool successfulApplication, bool pretend)
    {
        // TODO: Group effects by duration so they can be applied in proper order
        // TODO: Fire a message for effects that are DuringApplication, so they get unapplied as soon as the previous messages are processed
        Dictionary<EffectType, ResourceChangeAccumulation>? resourceChanges = null;
        foreach (var effectEntity in message.EffectsToApply)
        {
            // EffectsToApply is intentionally not reference-counted (see AbilityActivatedMessage),
            // so an effect entity may have been removed - its containing ability destroyed
            // between this message being enqueued and processed.
            // Skip effects that are no longer alive instead of dereferencing a pooled entity.
            if (effectEntity.Effect is not { ContainingAbilityId: not null } effect
                || effect.ContainingAbility?.Ability == null)
            {
                continue;
            }

            var metadata = Effect.GetMetadata(effect.EffectType);
            if (!successfulApplication && !metadata.AppliesOnFailure)
            {
                continue;
            }

            // Instant additive resource changes are folded by type and applied once. For damage, Subtract
            // selects the direction while same-school amounts sum; healing uses its authored Sum directly.
            var combinationFunction = effect.CombinationFunction;
            if (effect.Duration == EffectDuration.Instant
                && metadata.ResourceProperty != null
                && combinationFunction is ValueCombinationFunction.Subtract or ValueCombinationFunction.Sum)
            {
                resourceChanges ??= new Dictionary<EffectType, ResourceChangeAccumulation>();
                AccumulateRawResourceEffect(
                    resourceChanges, effectEntity,
                    GetAmount(effect, message.ActivatorEntity, message.TargetEntity));
            }
            else
            {
                ApplyEffect(effectEntity, message.TargetEntity, message.TargetCell, message.ActivatorEntity,
                    result, pretend);
            }
        }

        if (resourceChanges != null)
        {
            foreach (var resourceChange in resourceChanges)
            {
                ApplyEffect(
                    resourceChange.Value.EffectEntity!, message.TargetEntity, message.TargetCell,
                    message.ActivatorEntity, result, pretend,
                    resourceChange.Value.Amount);
            }
        }

    }

    private static void EnqueueResult(
        EffectResultMessage? result, AbilityActivatedMessage activation,
        AbilityComponent ability, bool successfulApplication, GameManager manager)
    {
        if (result == null)
        {
            return;
        }

        result.ActivatorEntity = activation.ActivatorEntity;
        result.TargetEntity = activation.TargetEntity!;
        switch (result)
        {
            case AttackResultMessage attackResult:
                attackResult.AbilityAction = ability.Action;
                attackResult.Ranged = (activation.Trigger & ActivationType.OnRangedAttack) != 0;
                attackResult.Hit = successfulApplication;
                attackResult.RevealsAttacker = (activation.Trigger & ActivationType.OnAttack) != 0;
                manager.Enqueue(attackResult);
                return;
            case ItemActivationResultMessage itemResult:
                itemResult.Successful = successfulApplication;
                manager.Enqueue(itemResult);
                return;
        }
    }

    private void ApplyEffect(
        GameEntity effectEntity,
        GameEntity? targetEntity,
        Point? targetCell,
        GameEntity activatorEntity,
        EffectResultMessage? result,
        bool pretend,
        int? resolvedAmount = null)
    {
        Debug.Assert(targetEntity?.ActiveEffects != null,
            $"Effect {effectEntity.Id} can only be applied to an affectable entity.");

        var effect = effectEntity.Effect!;
        var metadata = Effect.GetMetadata(effect.EffectType);
        metadata.ApplicationHandler(new EffectApplicationContext(
            this, effectEntity, targetEntity, targetCell, activatorEntity, result,
            pretend, resolvedAmount));
    }

    public void ApplyAbilityWithoutCarrier(Ability ability, GameEntity targetEntity)
    {
        foreach (var effect in ability.Effects ?? [])
        {
            ApplyEffectWithoutCarrier(effect, ability, targetEntity);
        }
    }

    private void ApplyEffectWithoutCarrier(Effect effect, Ability ability, GameEntity targetEntity)
    {
        if (effect is not IChangeProperty changeProperty
            || !effect.TargetActivator
            || effect.Duration != EffectDuration.Infinite)
        {
            throw new InvalidOperationException(
                $"Ability '{ability.Name}' can only be applied directly when all effects are permanent "
                + "self-targeted property changes.");
        }

        var application = new EffectApplication(effect, ability);
        var function = GetPropertyCombinationFunction(
            application.CombinationFunction, changeProperty.PropertyName, targetEntity.Manager);
        var instance = AddOrUpdateChangePropertyInstance(
            targetEntity, EffectType.ChangeProperty, changeProperty.PropertyName,
            function, sourceAbilityId: null,
            ability.Type, ability.Activation,
            application.GetAmount(this, targetEntity, targetEntity), targetEntity.Manager);
        TryRegisterDynamicMagnitude(application, instance, targetEntity, targetEntity);
    }

    private static void ValidateNonInstantApplication(EffectComponent effect)
    {
        if (effect.Duration == EffectDuration.Instant)
        {
            throw new NotSupportedException(
                $"Instant effect type {effect.EffectType} has no effect.");
        }
    }

    public void ExpireTransitionEffects(GameEntity entity, bool levelChanged, GameManager manager)
    {
        if (entity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        var propertyEffects = activeEffects.PropertyEffects;
        for (var i = propertyEffects.Count - 1; i >= 0; i--)
        {
            var expirationEvent = propertyEffects[i].ExpirationEvent;
            if (expirationEvent != EffectDuration.UntilNextRoom
                && (!levelChanged || expirationEvent != EffectDuration.UntilNextLevel))
            {
                continue;
            }

            if (RemovePropertyEffectAt(activeEffects, entity, i, manager) is { } property)
            {
                RaiseResolvedPropertyChange(property, entity, manager);
            }
        }

        var componentEffects = activeEffects.ComponentEffects;
        for (var i = componentEffects.Count - 1; i >= 0; i--)
        {
            if (i >= componentEffects.Count)
            {
                i = componentEffects.Count - 1;
            }

            var record = componentEffects[i];
            if (record.ExpirationEvent != EffectDuration.UntilNextRoom
                && (!levelChanged || record.ExpirationEvent != EffectDuration.UntilNextLevel))
            {
                continue;
            }

            var statusType = GetTimedStatusType(record);
            DetachComponentCarrier(record, entity, manager);
            if (statusType != null)
            {
                EnsureStatusAbility(statusType.Value, entity, manager);
            }

            i = Math.Min(i, componentEffects.Count);
        }

        RecomputeNextEffectTick(activeEffects);
    }

    private static void CombineExpiration(
        ActiveEffect existing, EffectExpiration applied, DurationPolicy policy, int currentTick)
    {
        if (policy == DurationPolicy.Replace
            || existing.ExpirationEvent != applied.Event)
        {
            existing.ExpirationEvent = applied.Event;
            existing.ExpirationTarget = applied.Target;
            return;
        }

        if (existing.ExpirationTarget == null || applied.Target == null)
        {
            return;
        }

        existing.ExpirationTarget = (existing.ExpirationEvent, policy) switch
        {
            (_, DurationPolicy.Refresh) => Math.Max(existing.ExpirationTarget.Value, applied.Target.Value),
            (EffectDuration.UntilTimeout, DurationPolicy.Extend) => (int)Math.Min(
                int.MaxValue,
                (long)existing.ExpirationTarget.Value + Math.Max(0, applied.Target.Value - currentTick)),
            (EffectDuration.UntilXPGained, DurationPolicy.Extend) => (int)Math.Min(
                int.MaxValue, (long)existing.ExpirationTarget.Value + applied.Target.Value),
            _ => applied.Target
        };
    }

    // Computes the expiry of a timed effect: UntilXPGained uses the raw duration amount as an XP budget for
    // players and converts it to ticks for non-players; every other timed effect expires
    // after the raw duration amount in ticks.
    private static EffectExpiration ComputeExpiration(
        EffectDuration duration, GameEntity targetEntity, int durationAmount, GameManager manager)
    {
        if (duration is EffectDuration.Instant or EffectDuration.Infinite or EffectDuration.DuringApplication)
        {
            return default;
        }

        if (duration is EffectDuration.UntilNextRoom or EffectDuration.UntilNextLevel)
        {
            return new EffectExpiration(duration);
        }

        if (duration == EffectDuration.UntilXPGained)
        {
            if (targetEntity.Player != null)
            {
                return new EffectExpiration(
                    EffectDuration.UntilXPGained,
                    durationAmount);
            }
            else
            {
                return new EffectExpiration(
                    EffectDuration.UntilTimeout,
                    manager.Game.CurrentTick + durationAmount);
            }
        }

        if (duration == EffectDuration.UntilTimeout)
        {
            return new EffectExpiration(
                EffectDuration.UntilTimeout,
                manager.Game.CurrentTick + durationAmount);
        }

        throw new ArgumentOutOfRangeException(nameof(duration), duration, null);
    }

    private EffectExpiration GetExpiration(
        EffectComponent effect, GameEntity activatorEntity, GameEntity targetEntity, GameManager manager)
    {
        var durationAmount = effect.Duration is EffectDuration.UntilTimeout or EffectDuration.UntilXPGained
            ? GetDurationAmount(effect, activatorEntity, targetEntity)
            : 0;
        return ComputeExpiration(effect.Duration, targetEntity, durationAmount, manager);
    }

    private static EffectExpiration GetExpiration(in EffectApplicationContext context)
    {
        var duration = context.Effect.Duration;
        var durationAmount = duration is EffectDuration.UntilTimeout or EffectDuration.UntilXPGained
            ? context.DurationAmount
            : 0;
        return ComputeExpiration(duration, context.TargetEntity!, durationAmount, context.Manager);
    }

    /// <summary>
    ///     Decrements XP-expiring property and component instances, removing each one whose budget runs out.
    ///     Invoked by <see cref="Knowledge.XPSystem" /> whenever the player gains experience.
    /// </summary>
    public void RealizeXpExpiringEffects(GameEntity playerEntity, int xp, GameManager manager)
    {
        var activeEffects = playerEntity.ActiveEffects!;

        var removedAny = false;
        HashSet<EffectType>? sourcePropagatedStatuses = null;
        ActiveEffectsComponent.ForEachEffect(
            activeEffects.PropertyEffects,
            state: 0,
            (instance, _) =>
            {
                if (instance is not
                    {
                        ExpirationEvent: EffectDuration.UntilXPGained,
                        ExpirationTarget: { } remaining
                    })
                {
                    return;
                }

                if (instance.Metadata!.PropagatesAmountToStatusAbility)
                {
                    (sourcePropagatedStatuses ??= []).Add(instance.EffectType);
                }

                var newExpiration = remaining - xp;
                if (newExpiration < 0)
                {
                    var index = activeEffects.PropertyEffects.IndexOf(instance);
                    if (index < 0)
                    {
                        return;
                    }

                    var property = RemovePropertyEffectAt(activeEffects, playerEntity, index, manager);
                    removedAny = true;
                    if (property != null)
                    {
                        RaiseResolvedPropertyChange(property, playerEntity, manager);
                    }
                }
                else
                {
                    instance.ExpirationTarget = newExpiration;
                }
            });

        if (sourcePropagatedStatuses != null)
        {
            foreach (var effectType in sourcePropagatedStatuses)
            {
                EnsureStatusAbility(effectType, playerEntity, manager);
            }
        }

        if (removedAny)
        {
            RecomputeNextEffectTick(activeEffects);
        }

        var componentEffects = activeEffects.ComponentEffects;
        for (var scanIndex = componentEffects.Count - 1; scanIndex >= 0; scanIndex--)
        {
            if (scanIndex >= componentEffects.Count)
            {
                scanIndex = componentEffects.Count - 1;
            }

            var record = componentEffects[scanIndex];
            if (record is not
                {
                    ExpirationEvent: EffectDuration.UntilXPGained,
                    ExpirationTarget: { } remaining
                })
            {
                continue;
            }

            var newExpiration = remaining - xp;
            if (newExpiration < 0)
            {
                var statusType = GetTimedStatusType(record);
                DetachComponentCarrier(record, playerEntity, manager);
                if (statusType != null)
                {
                    EnsureStatusAbility(statusType.Value, playerEntity, manager);
                }
            }
            else
            {
                record.ExpirationTarget = newExpiration;
            }
        }
    }

    /// <summary>
    ///     Removes every timed property or component effect on the entity whose expiration tick has passed,
    ///     then recomputes the owner's <see cref="ActiveEffectsComponent.NextEffectTick" />.
    /// </summary>
    public void ExpireTimedPropertyEffects(GameEntity entity, GameManager manager)
    {
        var activeEffects = entity.ActiveEffects!;

        var currentTick = manager.Game.CurrentTick;
        var instances = activeEffects.PropertyEffects;
        for (var scanIndex = instances.Count - 1; scanIndex >= 0; scanIndex--)
        {
            if (scanIndex >= instances.Count)
            {
                scanIndex = instances.Count - 1;
            }

            var instance = instances[scanIndex];
            if (instance is
                {
                    ExpirationEvent: EffectDuration.UntilTimeout,
                    ExpirationTarget: { } expiration
                }
                && expiration <= currentTick)
            {
                // A resource-over-time instance realizes its final partial interval as it lapses (the window
                // since the victim last acted, capped at the expiry).
                var metadata = instance.Metadata!;
                if (entity.Being is { IsAlive: true })
                {
                    var actor = (ActorComponent?)entity.AI ?? entity.Player;
                    if (actor?.PreviousActionTick is { } previousTick)
                    {
                        if (metadata.ResourceProperty != null)
                        {
                            ApplyLingeringResourceEffect(
                                instance, entity, expiration - previousTick, manager);
                        }
                        else if (instance.CombinationFunction.IsBuildUp())
                        {
                            ApplyLingeringBuildUpEffect(
                                instance, entity, expiration - previousTick, manager);
                        }
                    }
                }
                // A polymorph identity marker lapsing ends the transformation: drop the form's granted
                // abilities and identity (its stat overrides are reverted by the instance removals below).
                if (instance is { EffectType: EffectType.Polymorph, TargetName: null }
                    && entity.Being != null)
                {
                    EndPolymorph(entity, instance.MagnitudeSecondary, manager);
                }

                var index = instances.IndexOf(instance);
                if (index < 0)
                {
                    continue;
                }

                var property = RemovePropertyEffectAt(activeEffects, entity, index, manager);

                if (property != null)
                {
                    RaiseResolvedPropertyChange(property, entity, manager);
                }
            }
        }

        var componentEffects = activeEffects.ComponentEffects;
        for (var scanIndex = componentEffects.Count - 1; scanIndex >= 0; scanIndex--)
        {
            if (scanIndex >= componentEffects.Count)
            {
                scanIndex = componentEffects.Count - 1;
            }

            var record = componentEffects[scanIndex];
            if (record is not
                {
                    ExpirationEvent: EffectDuration.UntilTimeout,
                    ExpirationTarget: { } expiration
                }
                || expiration > currentTick)
            {
                continue;
            }

            var statusType = GetTimedStatusType(record);
            DetachComponentCarrier(record, entity, manager);
            if (statusType != null)
            {
                EnsureStatusAbility(statusType.Value, entity, manager);
            }
        }

        RecomputeNextEffectTick(activeEffects);
    }

    private static EffectType? GetTimedStatusType(ActiveComponentEffect record)
        => record is { EffectType: EffectType.AddAbility, SourceAbilityEntityId: 0 }
           && record.TargetName is { } addedName
           && Effect.FindMetadata(addedName) is { } metadata
            ? metadata.EffectType
            : null;

    // Advances NextEffectTick to surface an earlier due tick so TemporalEntitiesIndex reindexes the owner.
    private static void AdvanceNextEffectTick(
        ActiveEffectsComponent activeEffects, EffectExpiration expiration)
    {
        if (expiration is { Event: EffectDuration.UntilTimeout, Target: { } value }
            && (activeEffects.NextEffectTick == null || value < activeEffects.NextEffectTick.Value))
        {
            activeEffects.NextEffectTick = value;
        }
    }

    // Recomputes NextEffectTick as the earliest remaining timed-instance expiry (null if none).
    // SetWithNotify drives reindexing in TemporalEntitiesIndex.
    private static void RecomputeNextEffectTick(ActiveEffectsComponent activeEffects)
    {
        int? next = null;
        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance is
                {
                    ExpirationEvent: EffectDuration.UntilTimeout,
                    ExpirationTarget: { } expiration
                }
                && (next == null || expiration < next.Value))
            {
                next = expiration;
            }
        }

        foreach (var instance in activeEffects.ComponentEffects)
        {
            if (instance is
                {
                    ExpirationEvent: EffectDuration.UntilTimeout,
                    ExpirationTarget: { } expiration
                }
                && (next == null || expiration < next.Value))
            {
                next = expiration;
            }
        }

        activeEffects.NextEffectTick = next;
    }

    private readonly struct EffectExpiration(EffectDuration? expirationEvent, int? target = null)
    {
        public EffectDuration? Event { get; } = expirationEvent;
        public int? Target { get; } = target;
    }
}
