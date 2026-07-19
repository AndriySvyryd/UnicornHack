namespace UnicornHack.Systems.Effects;

// Storage and lifecycle operations for related to change property effects.
public partial class EffectApplicationSystem
{
    // A lingering property modifier is stored directly as an ActivePropertyEffect. A timed one (by ticks or
    // by player XP gained) self-expires; a source-linked (Infinite) one is keyed by its source ability +
    // property and lives until that ability deactivates or is removed. Instant/DuringApplication changes
    // apply and revert within the same application, so they are only reported.
    internal void ApplyChangeProperty(in EffectApplicationContext context)
    {
        var application = context.Effect;
        var effect = application.Component;
        ValidateNonInstantApplication(effect);

        if (!context.Pretend
            && effect.Duration is EffectDuration.UntilTimeout
                or EffectDuration.UntilXPGained
                or EffectDuration.UntilNextRoom
                or EffectDuration.UntilNextLevel)
        {
            var targetEntity = context.TargetEntity!;
            var activeEffects = targetEntity.ActiveEffects!;
            var amount = context.Amount;
            var secondaryAmount = context.SecondaryAmount;
            var sourceAbility = effect.ContainingAbility?.Ability;
            var activation = sourceAbility?.Activation ?? ActivationType.Default;

            var expiration = GetExpiration(context);

            var instance = AddTimedPropertyModifierInstance(
                activeEffects, targetEntity, effect.EffectType, amount, effect.TargetName!,
                GetPropertyCombinationFunction(
                    effect.CombinationFunction, effect.TargetName!, context.Manager),
                effect.ContainingAbilityId, activation,
                expiration.Target, context.Manager, secondaryAmount, expiration.Event);
            instance.SourceAbilityType = sourceAbility?.Type ?? AbilityType.Default;

            TryRegisterDynamicMagnitude(application, instance, targetEntity, context.ActivatorEntity);
        }
        else if (!context.Pretend
                 && effect.Duration == EffectDuration.Infinite)
        {
            var applicationAmount = new EffectApplication(effect);
            var property = effect.TargetName!;
            var sourceAbility = effect.ContainingAbility?.Ability;
            var instance = AddOrUpdateChangePropertyInstance(
                context.TargetEntity!, effect.EffectType, property,
                GetPropertyCombinationFunction(effect.CombinationFunction, property, context.Manager),
                effect.ContainingAbilityId ?? 0,
                sourceAbility?.Type ?? AbilityType.Default,
                sourceAbility?.Activation ?? ActivationType.Default,
                applicationAmount.GetAmount(this, context.ActivatorEntity, context.TargetEntity),
                context.Manager,
                effect.SecondaryAmount != null
                    ? GetSecondaryAmount(effect, context.ActivatorEntity, context.TargetEntity)
                    : 0);
            TryRegisterDynamicMagnitude(
                applicationAmount, instance, context.TargetEntity!, context.ActivatorEntity);
        }
        else
        {
            // TODO: Apply and revert DuringApplication property changes around the containing application.
            Debug.Assert(context.Pretend || effect.Duration == EffectDuration.DuringApplication);
        }
    }

    // Recomputes a resolved property and raises its change notification after its contributing modifier
    // instances have changed.
    private void RaiseResolvedPropertyChange(string property, GameEntity targetEntity, GameManager manager)
    {
        var propertyDescription = manager.Game.Services.Data.PropertyDescriptions.Find(property)!;
        UpdateResolvedProperty(propertyDescription, targetEntity);
    }

    private void InitializeProperties(GameEntity entity)
    {
        var properties = (List<PropertyDescription>)entity.Manager.Game.Services.Data.PropertyDescriptions.GetAsList();
        for (var i = 0; i < properties.Count; i++)
        {
            var propertyDescription = properties[i];
            if (entity.HasComponent(propertyDescription.ComponentId))
            {
                UpdateResolvedProperty(propertyDescription, entity);
            }
        }
    }

    // A dynamic-magnitude cascade recomputes an instance's magnitude, whose new value can shift another
    // property that yet another dynamic magnitude reads. Convergent cascades stop on their own (a recompute
    // that changes nothing propagates nothing); this cap only bounds a pathological amplifying cycle that
    // the self-reference guard at registration cannot catch (e.g. A -> B -> A).
    private const int MaxDynamicMagnitudeCascadeDepth = 16;

    // Recomputes a resolved property from the entity's ActiveEffects store and, if the resolved value
    // changed, raises the ECS property-value-changed notification (so subscribers such as
    // AbilitySlottingSystem react). The property is not persisted; its value is derived on read.
    private static void UpdateResolvedProperty(
        PropertyDescription propertyDescription,
        GameEntity targetEntity,
        int cascadeDepth = 0)
    {
        var activeEffects = targetEntity.ActiveEffects!;

        var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
        if (targetComponent == null)
        {
            return;
        }

        var isBool = propertyDescription.PropertyType == typeof(bool);
        if (!activeEffects.TryRefresh(propertyDescription.Name, isBool, out var oldValue, out var newValue))
        {
            return;
        }

        propertyDescription.RaiseResolvedChange(targetEntity, oldValue, newValue, targetComponent);

        // Push the change through the dynamic-magnitude dependency graph: every active instance whose cached
        // magnitude tracks this property is recomputed, and each output property whose contribution shifted is
        // re-resolved in turn, converging to a fixpoint (bounded by the depth cap against amplifying cycles).
        if (cascadeDepth >= MaxDynamicMagnitudeCascadeDepth
            || !activeEffects.HasDynamicMagnitudeDependents(propertyDescription.Name))
        {
            return;
        }

        var changedOutputs = new List<string>();
        activeEffects.RecomputeDynamicMagnitudes(propertyDescription.Name, changedOutputs);
        if (changedOutputs.Count == 0)
        {
            return;
        }

        var propertyDescriptions = targetEntity.Manager.Game.Services.Data.PropertyDescriptions;
        for (var i = 0; i < changedOutputs.Count; i++)
        {
            var outputDescription = propertyDescriptions.Find(changedOutputs[i]);
            if (outputDescription != null)
            {
                UpdateResolvedProperty(outputDescription, targetEntity, cascadeDepth + 1);
            }
        }
    }

    private static ValueCombinationFunction GetPropertyCombinationFunction(
        ValueCombinationFunction function, string property, GameManager manager)
        => function != ValueCombinationFunction.Default
            ? function
            : manager.Game.Services.Data.PropertyDescriptions.Find(property)!.DefaultCombinationFunction;

    /// <summary>
    ///     Removes every property-effect instance on <paramref name="activeEffects" /> matching
    ///     <paramref name="predicate" />, reverting the resolved properties they contributed to and
    ///     recomputing the owner's next effect tick. Returns whether any instance was removed.
    /// </summary>
    public bool RemoveMatchingPropertyEffects(
        ActiveEffectsComponent activeEffects, GameEntity targetEntity,
        Func<ActivePropertyEffect, bool> predicate, GameManager manager)
    {
        HashSet<string>? revertedProperties = null;
        var removedAny = false;
        for (var i = activeEffects.PropertyEffects.Count - 1; i >= 0; i--)
        {
            var instance = activeEffects.PropertyEffects[i];
            if (!predicate(instance))
            {
                continue;
            }

            if (RemovePropertyEffectAt(activeEffects, targetEntity, i, manager) is { } property)
            {
                (revertedProperties ??= []).Add(property);
            }
            removedAny = true;
        }

        if (!removedAny)
        {
            return false;
        }

        if (revertedProperties != null)
        {
            foreach (var property in revertedProperties)
            {
                RaiseResolvedPropertyChange(property, targetEntity, manager);
            }
        }

        RecomputeNextEffectTick(activeEffects);
        return true;
    }

    private string? RemovePropertyEffectAt(
        ActiveEffectsComponent activeEffects, GameEntity targetEntity, int index, GameManager manager)
    {
        var instance = activeEffects.PropertyEffects[index];
        var metadata = instance.Metadata!;
        var previousSource = instance.CombinationFunction == ValueCombinationFunction.BuildUpPerSource
            ? FindPerSourceBuildUpSource(targetEntity, instance.EffectType)
            : null;
        activeEffects.UnregisterDynamicMagnitude(instance);
        activeEffects.PropertyEffects.RemoveAt(index);
        ResetBuildUpOnSourceChange(instance.EffectType, previousSource, targetEntity);

        if (metadata.StatusAbilityName != null)
        {
            EnsureStatusAbility(instance.EffectType, targetEntity, manager);
        }

        return instance.TargetName;
    }

    private ActivePropertyEffect AddOrUpdateChangePropertyInstance(
        GameEntity targetEntity, EffectType effectType, string property,
        ValueCombinationFunction function, int? sourceAbilityId,
        AbilityType sourceAbilityType, ActivationType sourceAbilityActivation,
        int amount, GameManager manager, int secondaryAmount = 0)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var instances = activeEffects.PropertyEffects.GetValues(property);
        for (var i = 0; i < instances.Count; i++)
        {
            var instance = instances[i];
            if (instance.EffectType == effectType
                && instance.SourceAbilityEntityId == sourceAbilityId)
            {
                instance.SourceAbilityType = sourceAbilityType;
                instance.SourceAbilityActivation = sourceAbilityActivation;
                instance.MagnitudePrimary = amount;
                instance.MagnitudeSecondary = secondaryAmount;
                RaiseResolvedPropertyChange(property, targetEntity, manager);
                return instance;
            }
        }

        var addedInstance = activeEffects.AddPropertyEffect(
            effectType, property, function,
            sourceAbilityId, sourceAbilityType, sourceAbilityActivation,
            amount, secondaryAmount);

        RaiseResolvedPropertyChange(property, targetEntity, manager);
        return addedInstance;
    }

    public void HandleSourceAbilityRemoved(GameEntity affectedEntity, int abilityId, GameManager manager)
    {
        if (affectedEntity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        var instances = activeEffects.PropertyEffects;
        HashSet<string>? reverted = null;
        var removedAny = false;
        for (var i = instances.Count - 1; i >= 0; i--)
        {
            var instance = instances[i];
            if (instance.SourceAbilityEntityId != abilityId)
            {
                continue;
            }

            if (instance.EffectType == EffectType.Engulf
                || (instance.SourceAbilityActivation & ActivationType.Continuous) != 0)
            {
                if (RemovePropertyEffectAt(activeEffects, affectedEntity, i, manager) is { } property)
                {
                    (reverted ??= []).Add(property);
                }
                removedAny = true;
            }
            else
            {
                activeEffects.UnregisterDynamicMagnitude(instance);
                Debug.Assert(
                    instance.ExpirationEvent != null
                    || instance.CombinationFunction.IsBuildUp(),
                    $"Persisting effect {instance.EffectType} unlinked from ability {abilityId} on entity"
                    + $" {affectedEntity.Id} has no non-source expiration");

                instance.SourceAbilityEntityId = null;
            }
        }

        if (reverted != null)
        {
            foreach (var property in reverted)
            {
                RaiseResolvedPropertyChange(property, affectedEntity, manager);
            }
        }

        if (removedAny)
        {
            RecomputeNextEffectTick(activeEffects);
        }
    }

    public void UpdateOrAddPropertyModifier(
        int amount, string property, GameEntity entity, GameManager manager,
        ValueCombinationFunction function = ValueCombinationFunction.Sum)
    {
        var activeEffects = entity.ActiveEffects!;
        var instances = activeEffects.PropertyEffects.GetValues(property);

        ActivePropertyEffect? existing = null;
        for (var i = 0; i < instances.Count; i++)
        {
            var candidate = instances[i];
            if (candidate.SourceAbilityEntityId == null
                && candidate.SourceAbilityType == AbilityType.Innate
                && candidate.SourceAbilityActivation == ActivationType.Default
                && candidate.CombinationFunction == function)
            {
                existing = candidate;
                break;
            }
        }

        if (existing != null)
        {
            if (existing.MagnitudePrimary == amount)
            {
                return;
            }

            if (amount == 0
                && function == ValueCombinationFunction.Sum)
            {
                RemovePropertyEffectAt(
                    activeEffects, entity, activeEffects.PropertyEffects.IndexOf(existing), manager);
            }
            else
            {
                existing.MagnitudePrimary = amount;
            }
        }
        else
        {
            if (amount == 0
                && function == ValueCombinationFunction.Sum)
            {
                return;
            }

            activeEffects.AddPropertyEffect(
                EffectType.ChangeProperty, property, function,
                sourceAbilityType: AbilityType.Innate,
                magnitudePrimary: amount);
        }

        RaiseResolvedPropertyChange(property, entity, manager);
    }

    public void AddTimedPropertyOverride(
        int amount, string property, GameEntity entity, int duration, GameManager manager)
    {
        var activeEffects = entity.ActiveEffects!;
        var expirationTick = manager.Game.CurrentTick + duration;
        activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, property, ValueCombinationFunction.Override,
            sourceAbilityType: AbilityType.Status,
            magnitudePrimary: amount,
            expirationTarget: expirationTick,
            expirationEvent: EffectDuration.UntilTimeout);

        AdvanceNextEffectTick(
            activeEffects,
            new EffectExpiration(EffectDuration.UntilTimeout, expirationTick));
        RaiseResolvedPropertyChange(property, entity, manager);
    }

    internal ActivePropertyEffect AddTimedPropertyModifierInstance(
        ActiveEffectsComponent activeEffects, GameEntity entity, EffectType effectType, int amount, string property,
        ValueCombinationFunction function, int? sourceAbilityId, ActivationType activation,
        int? expirationTarget, GameManager manager, int secondaryAmount = 0,
        EffectDuration? expirationEvent = null)
    {
        var instance = activeEffects.AddPropertyEffect(
            effectType, property, function,
            sourceAbilityId, sourceAbilityActivation: activation,
            magnitudePrimary: amount, magnitudeSecondary: secondaryAmount,
            expirationTarget: expirationTarget, expirationEvent: expirationEvent);

        AdvanceNextEffectTick(
            activeEffects,
            new EffectExpiration(expirationEvent, expirationTarget));
        RaiseResolvedPropertyChange(property, entity, manager);

        return instance;
    }
}