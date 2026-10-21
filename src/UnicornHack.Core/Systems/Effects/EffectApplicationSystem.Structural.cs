using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects;

public partial class EffectApplicationSystem
{
    /// <summary>
    ///     Transforms <paramref name="targetEntity" /> into <paramref name="form" /> for
    ///     <paramref name="duration" /> ticks: lays the form's stats on as timed Override instances
    ///     (<see cref="Creature.ApplyFormOverride" />) and records a property-less identity marker
    ///     so the being resolves as the form (<see cref="BeingComponent.Species" />). Every
    ///     instance self-expires together, reverting the being to its own form. A polymorph replaces any
    ///     transformation already in force rather than stacking with it.
    /// </summary>
    public void ApplyPolymorph(Creature form, GameEntity targetEntity, int duration, GameManager manager)
    {
        var activeEffects = targetEntity.ActiveEffects!;

        // Replace, don't stack: tear down any transformation already in force before laying on the new one.
        RemoveExistingPolymorph(targetEntity, manager);

        var being = targetEntity.Being!;
        var formAbilityTemplate = form.GetRaceAbilityTemplate();
        foreach (var raceEntity in being.Races)
        {
            if (ReferenceEquals(raceEntity.Ability?.Template, formAbilityTemplate))
            {
                return;
            }
        }

        var expirationTick = manager.Game.CurrentTick + duration;

        // Identity marker: a property-less Polymorph instance carrying the form's loader id, read by
        // BeingComponent.Species. It self-expires alongside the stat overrides.
        activeEffects.AddPropertyEffect(
            EffectType.Polymorph,
            magnitudeSecondary: Creature.Loader.GetId(form.Name!),
            expirationTarget: expirationTick,
            expirationEvent: EffectDuration.UntilTimeout);

        AdvanceNextEffectTick(
            activeEffects,
            new EffectExpiration(EffectDuration.UntilTimeout, expirationTick));

        form.ApplyFormOverride(targetEntity, duration, manager);

        // The form's own Race ability carries the suppressor, so DecreaseAbilityLevel's source exclusion
        // keeps the form active while lowering every other Race below zero. Removing the form carrier
        // removes this source-linked decrement and restores the original races automatically.
        var formRecord = manager.AbilityLevelSystem.AddAbilityEffect(targetEntity, formAbilityTemplate);
        var formAbility = manager.FindEntity(formRecord.CarrierId)!.Ability!;
        var raceSuppression = new DecreaseAbilityLevel
        {
            TargetActivator = true,
            AbilityType = AbilityType.Race,
            Amount = "3",
            ContainingAbility = formAbility.Template!
        };
        raceSuppression.AddToAbility(formAbility, manager);
    }

    // Ends a polymorph transformation by removing the form race carrier. Its source-linked Race decrement
    // disappears during deactivation, restoring the original races; stat overrides expire separately.
    private void EndPolymorph(GameEntity entity, int formId, GameManager manager)
    {
        if (Creature.Loader.FindById(formId) is not { } form
            || FindRaceComponentEffect(entity, form.GetRaceAbilityName()) is not { } formRecord)
        {
            return;
        }

        var formCarrierId = formRecord.CarrierId;

        // Detaching the carrier alone does not drop the abilities it granted (their AddAbility directives
        // live on the being, sourced by the carrier), so remove those first, then detach the carrier to
        // clear the form's being.Races identity.
        manager.AbilityLevelSystem.RemoveAbilityComponentEffects(entity, formCarrierId, manager);
        AbilityLevelSystem.DetachComponentCarrier(formRecord, entity, manager);

        static ActiveComponentEffect? FindRaceComponentEffect(GameEntity entity, string raceAbilityName)
        {
            foreach (var record in entity.ActiveEffects!.ComponentEffects.GetValues(raceAbilityName))
            {
                if (record.EffectType == EffectType.AddAbility
                    && record.SourceAbilityEntityId == 0)
                {
                    return record;
                }
            }

            return null;
        }
    }

    // Tears down every polymorph transformation currently in force on the entity so a freshly applied one
    // replaces rather than stacks with it: ends each form (dropping its granted abilities and carrier) and
    // removes the transformation's instances, reverting the stats they forced. The being's own race
    // abilities are deliberately left suppressed - the replacement keeps them so - and are restored only when
    // a transformation lapses on its own.
    private void RemoveExistingPolymorph(GameEntity entity, GameManager manager)
    {
        if (entity.Being == null)
        {
            return;
        }

        var activeEffects = entity.ActiveEffects!;

        var instances = activeEffects.PropertyEffects;

        List<(int FormId, int? ExpirationTarget)>? forms = null;
        foreach (var instance in instances)
        {
            if (instance is { EffectType: EffectType.Polymorph, TargetName: null })
            {
                (forms ??= []).Add((instance.MagnitudeSecondary, instance.ExpirationTarget));
            }
        }

        if (forms == null)
        {
            return;
        }

        foreach (var form in forms)
        {
            EndPolymorph(entity, form.FormId, manager);
        }

        // Drop the transformation's own instances (identity markers and stat overrides), reverting the
        // stats they forced.
        RemoveMatchingPropertyEffects(
            activeEffects, entity,
            instance => instance.EffectType == EffectType.Polymorph
                        || instance is
                        {
                            EffectType: EffectType.ChangeProperty,
                            CombinationFunction: ValueCombinationFunction.Override,
                            SourceAbilityEntityId: null,
                            SourceAbilityType: AbilityType.Debuff,
                            ExpirationEvent: EffectDuration.UntilTimeout
                        }
                        && forms.Any(form => form.ExpirationTarget == instance.ExpirationTarget),
            manager);
    }
}
