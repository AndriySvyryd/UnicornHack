using System.Globalization;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Effects;

public partial class EffectApplicationSystem
{
    // Any effect that carries a duration and lingers as a status: banked as an ActivePropertyEffect so it
    // persists and other systems can react to it (e.g. denying actions while stunned). Adding a new status
    // effect therefore needs no dedicated case in ApplyEffect.
    public void ApplyStatus(in EffectApplicationContext context)
    {
        var application = context.Effect;
        var effectType = application.EffectType;
        var combinationFunction = application.CombinationFunction;
        var isBuildUp = combinationFunction.IsBuildUp();
        if (!isBuildUp
            && application.HasComponent)
        {
            ValidateNonInstantApplication(application.Component);
        }

        // A lingering status the target's defenses turn aside takes no hold and is not reported -
        // neither on a real application nor on a preview. Everything else is reported; only a real,
        // unblocked lingering status is actually banked.
        if (context.TargetEntity is { } target
            && !context.Pretend)
        {
            var amount = GetEffectiveStatusAmount(context, combinationFunction);
            var secondaryAmount = context.SecondaryAmount;

            EffectExpiration expiration = default;
            if (application.Duration != EffectDuration.Instant)
            {
                expiration = GetExpiration(context);
            }

            var sourceAbilityId = combinationFunction == ValueCombinationFunction.BuildUpPerSource
                                  || !application.HasComponent
                ? application.SourceAbilityId
                : null;
            if (isBuildUp && application.HasComponent)
            {
                if (application.Duration != EffectDuration.Instant)
                {
                    var previousSourceAbilityId = FindPerSourceBuildUpSource(
                        target, effectType)?.SourceAbilityEntityId;
                    AddBuildUpSource(
                        application.Component, target, amount, secondaryAmount, expiration,
                        sourceAbilityId, context.Manager);
                    if (ResetBuildUpOnSourceChange(effectType, previousSourceAbilityId, target))
                    {
                        EnsureStatusAbility(effectType, target, context.Manager);
                    }

                    return;
                }

                if (sourceAbilityId != null)
                {
                    var previousSourceAbilityId = FindPerSourceBuildUpSource(
                        target, effectType)?.SourceAbilityEntityId;
                    if (combinationFunction == ValueCombinationFunction.BuildUpPerSource)
                    {
                        RemoveNonMatchingPerSourceBuildUpSources(
                            effectType, sourceAbilityId, target, context.Manager);
                    }

                    if (combinationFunction != ValueCombinationFunction.BuildUpPerSource
                        || !HasPerSourceBuildUpSource(
                            target, effectType, sourceAbilityId.Value))
                    {
                        AddBuildUpSource(
                            application.Component, target, amount: 0, secondaryAmount,
                            expiration: default,
                            sourceAbilityId, context.Manager);
                    }

                    ResetBuildUpOnSourceChange(effectType, previousSourceAbilityId, target);
                }
            }
            else if (isBuildUp && sourceAbilityId != null)
            {
                var previousSourceAbilityId = FindPerSourceBuildUpSource(
                    target, effectType)?.SourceAbilityEntityId;
                if (combinationFunction == ValueCombinationFunction.BuildUpPerSource)
                {
                    RemoveNonMatchingPerSourceBuildUpSources(
                        effectType, sourceAbilityId, target, context.Manager);
                }

                AddDirectBuildUpSource(
                    effectType, target, sourceAbilityId.Value);
                ResetBuildUpOnSourceChange(effectType, previousSourceAbilityId, target);
            }

            ApplyBankedStatus(
                effectType, combinationFunction, target, amount, secondaryAmount,
                expiration, sourceAbilityId, context.Manager);
        }
    }

    internal int GetEffectiveStatusAmount(
        in EffectApplicationContext context, ValueCombinationFunction? combinationFunction = null)
    {
        var target = context.TargetEntity!;
        var application = context.Effect;
        var function = combinationFunction ?? application.CombinationFunction;
        var amount = context.Amount;
        return function.IsBuildUp()
            ? CalculateEffectiveAmount(
                application.EffectType, application.HasComponent ? application.Component : null,
                context.ActivatorEntity, target, amount, function)
            : amount;
    }

    // The shared reaction for a lingering (ability-template) or inflicted (damage-rider) status, sans the
    // resource-over-time and property-changing types. Callers supply the resolved expiry/magnitude and source ability.
    private void ApplyBankedStatus(
        EffectType effectType, ValueCombinationFunction combinationFunction,
        GameEntity targetEntity, int amount, int secondaryAmount,
        EffectExpiration expiration, int? sourceAbilityId, GameManager manager)
    {
        var metadata = Effect.GetMetadata(effectType);
        if (combinationFunction.IsBuildUp())
        {
            BankBuildUp(effectType, amount, targetEntity, manager);
        }
        else if (metadata.StatusAbilityName != null)
        {
            var activeEffects = targetEntity.ActiveEffects!;
            activeEffects.AddPropertyEffect(
                effectType,
                combinationFunction: combinationFunction,
                sourceAbilityEntityId: sourceAbilityId,
                sourceAbilityActivation: sourceAbilityId.HasValue
                    ? manager.FindEntity(sourceAbilityId.Value)!.Ability?.Activation ?? ActivationType.Default
                    : ActivationType.Default,
                magnitudePrimary: amount,
                magnitudeSecondary: secondaryAmount,
                expirationTarget: expiration.Target,
                expirationEvent: expiration.Event);
            AdvanceNextEffectTick(activeEffects, expiration);
            EnsureStatusAbility(effectType, targetEntity, manager);
        }
    }

    private static void AddBuildUpSource(
        EffectComponent effect, GameEntity targetEntity, int amount, int secondaryAmount,
        EffectExpiration expiration, int? sourceAbilityId, GameManager manager)
    {
        if (amount <= 0 && sourceAbilityId == null)
        {
            return;
        }

        var metadata = Effect.GetMetadata(effect.EffectType);
        var activeEffects = targetEntity.ActiveEffects!;
        if (metadata.DurationPolicy != DurationPolicy.Independent)
        {
            foreach (var existing in activeEffects.PropertyEffects)
            {
                if (existing.EffectType == effect.EffectType
                    && existing.CombinationFunction == effect.CombinationFunction
                    && existing.SourceAbilityEntityId == sourceAbilityId)
                {
                    existing.MagnitudePrimary = amount;
                    existing.MagnitudeSecondary = secondaryAmount;
                    CombineExpiration(
                        existing, expiration, metadata.DurationPolicy, manager.Game.CurrentTick);
                    RecomputeNextEffectTick(activeEffects);
                    return;
                }
            }
        }

        activeEffects.AddPropertyEffect(
            effect.EffectType,
            combinationFunction: effect.CombinationFunction,
            sourceAbilityEntityId: sourceAbilityId,
            sourceAbilityActivation: effect.ContainingAbility?.Ability?.Activation ?? ActivationType.Default,
            magnitudePrimary: amount,
            magnitudeSecondary: secondaryAmount,
            expirationTarget: expiration.Target,
            expirationEvent: expiration.Event);
        AdvanceNextEffectTick(activeEffects, expiration);
    }

    private static void AddDirectBuildUpSource(
        EffectType effectType, GameEntity targetEntity, int sourceAbilityId)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        foreach (var existing in activeEffects.PropertyEffects)
        {
            if (existing.EffectType == effectType
                && existing.SourceAbilityEntityId == sourceAbilityId)
            {
                return;
            }
        }

        activeEffects.AddPropertyEffect(
            effectType,
            combinationFunction: Effect.GetMetadata(effectType).CombinationFunction,
            sourceAbilityEntityId: sourceAbilityId);
    }

    private void RemoveNonMatchingPerSourceBuildUpSources(
        EffectType effectType, int? sourceAbilityId, GameEntity targetEntity, GameManager manager)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var instances = activeEffects.PropertyEffects;
        var removedAny = false;
        for (var i = instances.Count - 1; i >= 0; i--)
        {
            var instance = instances[i];
            if (instance.EffectType == effectType
                && instance.CombinationFunction == ValueCombinationFunction.BuildUpPerSource
                && instance.SourceAbilityEntityId != sourceAbilityId)
            {
                if (instance is
                    {
                        ExpirationEvent: EffectDuration.UntilTimeout,
                        ExpirationTarget: { } expirationTick
                    }
                    && expirationTick == activeEffects.NextEffectTick)
                {
                    removedAny = true;
                }

                RemovePropertyEffectAt(activeEffects, targetEntity, i, manager);
            }
        }

        if (removedAny)
        {
            RecomputeNextEffectTick(activeEffects);
        }
    }

    private static bool ResetBuildUpOnSourceChange(
        EffectType effectType, int? previousSourceAbilityId, GameEntity targetEntity)
    {
        if (previousSourceAbilityId == null
            || FindPerSourceBuildUpSource(targetEntity, effectType) is not { } currentSource
            || previousSourceAbilityId == currentSource.SourceAbilityEntityId)
        {
            return false;
        }

        if (targetEntity.ActiveEffects!.Accumulators.GetValueOrDefault(effectType) is { } accumulator)
        {
            accumulator.Value = 0;
            return true;
        }

        return false;
    }

    public static ActivePropertyEffect? FindPerSourceBuildUpSource(
        GameEntity entity, EffectType effectType)
    {
        ActivePropertyEffect? strongest = null;
        foreach (var instance in entity.ActiveEffects!.PropertyEffects)
        {
            if (instance.EffectType == effectType
                && instance.CombinationFunction == ValueCombinationFunction.BuildUpPerSource
                && (strongest == null || instance.MagnitudePrimary > strongest.MagnitudePrimary))
            {
                strongest = instance;
            }
        }

        return strongest;
    }

    private static bool HasPerSourceBuildUpSource(
        GameEntity entity, EffectType effectType, int sourceAbilityId)
    {
        foreach (var instance in entity.ActiveEffects!.PropertyEffects)
        {
            if (instance.EffectType == effectType
                && instance.CombinationFunction == ValueCombinationFunction.BuildUpPerSource
                && instance.SourceAbilityEntityId == sourceAbilityId)
            {
                return true;
            }
        }

        return false;
    }

    public static (EffectDuration? Event, int? Target) GetStatusAbilityExpiration(
        EffectMetadata metadata, GameEntity targetEntity, int level)
    {
        if (metadata.Duration is EffectDuration.UntilNextRoom or EffectDuration.UntilNextLevel)
        {
            return (metadata.Duration, null);
        }

        if (metadata.StatusAbilityDuration == null
            || metadata.GetStatusAbilityDuration(targetEntity, level) is not { } duration)
        {
            return default;
        }

        var expiration = ComputeExpiration(
            metadata.Duration == EffectDuration.UntilXPGained
                ? EffectDuration.UntilXPGained
                : EffectDuration.UntilTimeout,
            targetEntity, duration, targetEntity.Manager);
        return (expiration.Event, expiration.Target);
    }

    /// <summary>
    ///     Realizes every threshold build-up whose banked magnitude evolves with elapsed time at the start of
    ///     the actor's turn, folding the former DecayStatusEffect and RealizeCounters passes into one:
    ///     ordinary residual build-up fades at a rate determined by the active status level and is removed at
    ///     zero. Effect-specific consequences such as doom, overcharge, and lycanthropy are delegated through
    ///     their metadata after generic evolution.
    /// </summary>
    public void RealizeBuildUps(GameEntity actorEntity, int elapsed, GameManager manager)
    {
        if (actorEntity.Being is not { IsAlive: true })
        {
            return;
        }

        var activeEffects = actorEntity.ActiveEffects!;

        // Build-up evolution is metadata-driven. Round a partial action interval up so a build-up with a
        // non-zero decay evolves on every turn where time passed; zero elapsed time never changes magnitude.
        var intervals = elapsed > 0
            ? (elapsed + TimeSystem.DefaultActionDelay - 1) / TimeSystem.DefaultActionDelay
            : 0;

        ActiveEffectsComponent.ForEachEffect(
            activeEffects.Accumulators,
            (System: this, ActorEntity: actorEntity, Manager: manager, Intervals: intervals),
            static (accumulator, state) =>
            {
                if (state.ActorEntity.Being is not { IsAlive: true }
                    || Effect.GetMetadata(accumulator.EffectType).BuildUpRealizationHandler is not { } realize)
                {
                    return;
                }

                realize(new BuildUpRealizationContext(
                    state.System, accumulator, state.ActorEntity, state.Manager, state.Intervals));
            });
    }

    public bool RealizeBuildUp(in BuildUpRealizationContext context, int decay)
    {
        var accumulator = context.Accumulator;
        var actorEntity = context.ActorEntity;
        var activeEffects = actorEntity.ActiveEffects!;

        ApplyBuildUpDecay(
            activeEffects, actorEntity, accumulator, context.Intervals, decay, context.Manager);
        return activeEffects.Accumulators.ContainsKey(accumulator.EffectType);
    }

    // Applies the effect type's signed per-interval decay, dropping a spent accumulator and synchronizing
    // any status ability it controls. Returns whether a non-zero decay was applied.
    private bool ApplyBuildUpDecay(
        ActiveEffectsComponent activeEffects, GameEntity actorEntity, EffectAccumulator accumulator, int intervals,
        int decay, GameManager manager)
    {
        if (intervals == 0
            || decay == 0)
        {
            return false;
        }

        accumulator.Value -= decay * intervals;
        if (accumulator.Value <= 0)
        {
            var effectType = accumulator.EffectType;
            activeEffects.Accumulators.Remove(accumulator);
            EnsureStatusAbility(effectType, actorEntity, manager);
            return true;
        }

        EnsureStatusAbility(accumulator.EffectType, actorEntity, manager);
        return true;
    }

    public void ApplyLingeringBuildUpEffect(
        ActivePropertyEffect activeEffect, GameEntity targetEntity, int elapsed, GameManager manager)
    {
        if (elapsed <= 0)
        {
            return;
        }

        // Only the strongest source contributes to a per-source build-up; the others are ignored until they become the strongest.
        if (activeEffect.CombinationFunction == ValueCombinationFunction.BuildUpPerSource
            && !ReferenceEquals(
                activeEffect, FindPerSourceBuildUpSource(targetEntity, activeEffect.EffectType)))
        {
            return;
        }

        var contribution = activeEffect.MagnitudePrimary * elapsed / TimeSystem.DefaultActionDelay;
        if (contribution <= 0)
        {
            return;
        }

        BankBuildUp(activeEffect.EffectType, contribution, targetEntity, manager);
    }

    // Applies an effect's inflicted-status riders from the amount that actually changed its resource.
    // Periodic effects repeat these riders on each pulse; the final partial expiry pulse omits them.
    private void ApplyRiders(
        EffectType effectType, int amount, GameEntity targetEntity, GameManager manager)
    {
        if (targetEntity.Being == null
            || amount <= 0)
        {
            return;
        }

        ApplyInflictedStatus(effectType, amount, targetEntity, manager);
    }

    // Keeps a status ability aligned with either its build-up accumulator or its resource-over-time instances.
    private void EnsureStatusAbility(
        EffectType effectType, GameEntity targetEntity, GameManager manager)
    {
        var metadata = Effect.GetMetadata(effectType);
        if (metadata.StatusAbilityName is not { } abilityName)
        {
            return;
        }

        var activeEffects = targetEntity.ActiveEffects!;
        var isBuildUp = metadata.CombinationFunction.IsBuildUp();
        var isPerSource = metadata.CombinationFunction == ValueCombinationFunction.BuildUpPerSource;
        if (isPerSource
            && FindPerSourceBuildUpSource(targetEntity, effectType) == null)
        {
            RemoveAbility(
                targetEntity, abilityName, manager, sourceAbilityEntityId: 0);
            return;
        }

        var template = manager.Game.Services.Data.Abilities.Get(abilityName);
        var currentLevel = GetComponentEffectLevel(
            targetEntity, abilityName, EffectType.AddAbility, sourceAbilityEntityId: 0,
            onlyUnconditional: true);
        var levelsToAdd = 0;

        if (isBuildUp)
        {
            if (activeEffects.Accumulators.GetValueOrDefault(effectType) is { } accumulator)
            {
                var threshold = GetBuildUpThreshold(metadata.CombinationFunction, targetEntity);
                levelsToAdd = accumulator.Value / threshold;
                accumulator.Value -= levelsToAdd * threshold;
            }
        }
        else
        {
            var desiredLevel = template is LeveledAbility
                ? SumActiveEffectAmounts(effectType, activeEffects) / BuildUpThreshold
                : HasPropertyEffect(effectType, activeEffects) ? 1 : 0;
            if (metadata.StatusAbilityDuration != null)
            {
                desiredLevel = Math.Max(desiredLevel, currentLevel);
            }

            if (desiredLevel < currentLevel)
            {
                RemoveAbilityLevels(
                    abilityName, currentLevel - desiredLevel, sourceAbilityEntityId: 0,
                    targetEntity, manager);
                currentLevel = desiredLevel;
            }

            levelsToAdd = desiredLevel - currentLevel;
        }

        if (levelsToAdd > 0)
        {
            AddStatusAbilityLevels(
                effectType, metadata, abilityName, template,
                currentLevel, levelsToAdd, targetEntity, manager);
            LogStatusActivated(
                targetEntity, effectType, abilityName, currentLevel + levelsToAdd, manager);
        }

        if (metadata.PropagatesAmountToStatusAbility)
        {
            SynchronizeStatusAbilityAmounts(effectType, abilityName, targetEntity, manager);
        }
    }

    private void AddStatusAbilityLevels(
        EffectType effectType, EffectMetadata metadata, string abilityName, Ability template,
        int currentLevel, int levelsToAdd, GameEntity targetEntity, GameManager manager)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var newLevel = currentLevel + levelsToAdd;

        var existingLevel = 0;
        foreach (var record in GetOrderedComponentEffectRecords(
                     abilityName, targetEntity, EffectType.AddAbility, sourceAbilityEntityId: 0,
                     onlyUnconditional: true))
        {
            existingLevel += record.Level;
            var expiration = GetStatusAbilityExpiration(metadata, targetEntity, existingLevel);
            record.ExpirationTarget = expiration.Target;
            record.ExpirationEvent = expiration.Event;
        }

        for (var level = currentLevel + 1; level <= newLevel; level++)
        {
            var expiration = GetStatusAbilityExpiration(metadata, targetEntity, level);
            AddComponentEffectRecord(
                targetEntity, EffectType.AddAbility, abilityName,
                sourceAbilityEntityId: 0, level: 1,
                expirationTarget: expiration.Target,
                expirationEvent: expiration.Event);
        }

        RecomputeNextEffectTick(activeEffects);
        RelevelAbility(abilityName, targetEntity, manager, template);
    }

    private void SynchronizeStatusAbilityAmounts(
        EffectType effectType, string abilityName, GameEntity targetEntity, GameManager manager)
    {
        var activeEffects = targetEntity.ActiveEffects!;
        var record = FindComponentEffect(
            targetEntity, EffectType.AddAbility, abilityName, sourceAbilityEntityId: 0,
            onlyUnconditional: true);

        if (record == null)
        {
            return;
        }

        var metadata = Effect.GetMetadata(effectType);

        if (manager.FindEntity(record.CarrierId) is not { Ability: { } ability } abilityEntity)
        {
            return;
        }

        var (amount, secondaryAmount) = GetPropagatedStatusAmounts(effectType, metadata, activeEffects);

        var requiresRebuild = false;
        foreach (var effectEntity in ability.Effects)
        {
            if (effectEntity.Effect is not { } statusEffect)
            {
                continue;
            }

            if ((statusEffect.EffectType == EffectType.ChangeProperty
                 && statusEffect.Amount != amount)
                || (secondaryAmount != null
                    && statusEffect.SecondaryAmount != secondaryAmount))
            {
                requiresRebuild = true;
                break;
            }
        }

        if (!requiresRebuild)
        {
            return;
        }

        RelevelAbility(
            abilityName, targetEntity, manager,
            manager.Game.Services.Data.Abilities.Get(abilityName));
    }

    private static (string Amount, string? SecondaryAmount) GetPropagatedStatusAmounts(
        EffectType effectType, EffectMetadata metadata, ActiveEffectsComponent activeEffects)
    {
        var amount = metadata.CombinationFunction.IsBuildUp()
            ? activeEffects.Accumulators.GetValueOrDefault(effectType)?.Value ?? 0
            : SumActiveEffectAmounts(effectType, activeEffects);
        int? secondaryAmount = null;
        foreach (var effect in activeEffects.PropertyEffects)
        {
            if (effect.EffectType == effectType
                && effect.MagnitudeSecondary != 0)
            {
                secondaryAmount = effect.MagnitudeSecondary;
                break;
            }
        }

        return (
            amount.ToString(CultureInfo.InvariantCulture),
            secondaryAmount?.ToString(CultureInfo.InvariantCulture));
    }

    private static int SumActiveEffectAmounts(
        EffectType effectType, ActiveEffectsComponent activeEffects)
    {
        var value = 0L;
        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance.EffectType == effectType)
            {
                value += instance.MagnitudePrimary;
            }
        }

        return (int)Math.Clamp(value, int.MinValue, int.MaxValue);
    }

    private static bool HasPropertyEffect(EffectType effectType, ActiveEffectsComponent activeEffects)
    {
        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance.EffectType == effectType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Applies the lingering status that a damage type inflicts on a hit (e.g. acid corrodes,
    ///     frost chills, toxin decays, a slash bleeds), if any, so abilities dealing that damage don't
    ///     each carry a duplicate rider. A self-lingering damage-over-time secondary keeps damaging at
    ///     the rate of the blow that caused it (<paramref name="inflictedDamage" />); a distinct status
    ///     carries its fixed strength from <see cref="DamageInflictedStatuses" />. A harder hit contributes
    ///     more build-up.
    /// </summary>
    private void ApplyInflictedStatus(
        EffectType damageType, int inflictedDamage, GameEntity targetEntity, GameManager manager)
    {
        var metadata = Effect.GetMetadata(damageType);
        if (metadata.CombinationFunction != ValueCombinationFunction.Subtract
            || metadata.Inflicts is not { } inflictedStatuses
            || targetEntity.Being == null)
        {
            return;
        }

        var activeEffects = targetEntity.ActiveEffects!;
        foreach (var status in inflictedStatuses)
        {
            if (status.Clears)
            {
                RemoveMatchingPropertyEffects(
                    activeEffects, targetEntity,
                    instance => instance.EffectType == status.Status, manager);
                if (activeEffects.Accumulators.GetValueOrDefault(status.Status) is { } accumulator)
                {
                    activeEffects.Accumulators.Remove(accumulator);
                }

                if (Effect.GetMetadata(status.Status).StatusAbilityName is { } clearedAbilityName)
                {
                    RemoveAbility(
                        targetEntity, clearedAbilityName, manager,
                        sourceAbilityEntityId: 0);
                }

                continue;
            }

            var amount = inflictedDamage * status.AmountFactor / 10;
            if (amount <= 0)
            {
                continue;
            }

            ApplyStatus(EffectApplicationContext.CreateStatus(status.Status, targetEntity, amount));
        }
    }

    /// <summary>
    ///     The fixed build-up at which a status takes hold. Doom instead takes hold against the victim's
    ///     remaining health, so it does not use this constant.
    /// </summary>
    public const int BuildUpThreshold = 100;

    /// <summary>
    ///     The banked total a build-up accumulator of the given variant must reach to take hold on the
    ///     target: its current health for <see cref="ValueCombinationFunction.BuildUpHealth" />, otherwise
    ///     the fixed <see cref="BuildUpThreshold" />. Per-source build-up additionally requires an active
    ///     source marker when realized.
    /// </summary>
    public static int GetBuildUpThreshold(ValueCombinationFunction function, GameEntity entity)
        => function switch
        {
            ValueCombinationFunction.BuildUpHealth => entity.Being?.HitPoints ?? BuildUpThreshold,
            _ => BuildUpThreshold
        };

    // Combines a fresh contribution with a build-up accumulator's existing banked total per the effect type's
    // duration policy (see EffectMetadata.DurationPolicy) - the single axis every re-application of the same type
    // follows: Refresh keeps the longer timer, Replace resets to the new value, and Extend accumulates.
    // Independent never reaches here, since those types bank a separate instance instead of merging.
    private static int CombineBuildUpMagnitude(int existing, int contribution, DurationPolicy policy)
        => policy switch
        {
            DurationPolicy.Refresh => Math.Max(existing, contribution),
            DurationPolicy.Replace => contribution,
            _ => existing + contribution
        };

    // Folds a contribution into the target's value-only build-up accumulator and refreshes its status.
    public void BankBuildUp(
        EffectType effectType, int contribution, GameEntity targetEntity, GameManager manager)
    {
        if (contribution <= 0)
        {
            // Fully resisted or zero-length: it never takes hold.
            return;
        }

        var metadata = Effect.GetMetadata(effectType);
        var activeEffects = targetEntity.ActiveEffects!;

        // Fold this application into an existing build-up of the same type so successive applications
        // accumulate toward the action interval, and the leftover banked after an in-force action
        // counts toward taking hold again. The accumulator persists and is spent an action interval at a
        // time (see RealizeBuildUps) instead of expiring on its own.
        var accumulator = activeEffects.Accumulators.GetValueOrDefault(effectType);
        if (accumulator == null)
        {
            accumulator = activeEffects.AddAccumulator(effectType);
        }

        // Combine this application's banked contribution with the accumulator per the effect type's duration
        // policy: the build-up incapacitations accumulate toward the threshold (Extend), while a plain status
        // keeps the longer remaining timer instead of piling up (Refresh).
        if (contribution > 0)
        {
            accumulator.Value = CombineBuildUpMagnitude(
                accumulator.Value, contribution, metadata.DurationPolicy);
        }

        EnsureStatusAbility(effectType, targetEntity, manager);
    }

    // Announces to each player who can sense the target that a status condition has just taken hold on it,
    // telling the afflicted player how it affects them.
    private void LogStatusActivated(
        GameEntity targetEntity, EffectType effectType, string statusName, int abilityLevel, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            var targetSensed = manager.SensorySystem.CanSense(playerEntity, targetEntity) ?? SenseType.None;
            if (targetSensed == SenseType.None)
            {
                continue;
            }

            var logMessage = manager.Game.Services.Language.GetString(
                new StatusActivatedEvent(
                    playerEntity, targetEntity, effectType, statusName, abilityLevel, targetSensed));
            manager.LoggingSystem.WriteLog(logMessage, playerEntity, manager);
        }
    }
}