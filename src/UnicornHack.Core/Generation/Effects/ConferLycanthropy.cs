using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class ConferLycanthropy : Effect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.ConferLycanthropy, AbilityData.Lycanthropic.Name,
            statusCategory: StatusCategory.Physical,
            combinationFunction: ValueCombinationFunction.BuildUpFixed,
            durationPolicy: DurationPolicy.Extend,
            defaultAmount: "100*MightModifier()",
            statusAbilityDuration: "$level == 1 ? null : 2000",
            applicationHandler: Apply,
            propagatesAmountToStatusAbility: true);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        if (context.TargetEntity?.Being == null
            || context.Pretend)
        {
            return;
        }

        var activeEffects = context.TargetEntity.ActiveEffects!;
        var lycanthropyForm = Polymorph.FindPolymorphForm(context);
        if (lycanthropyForm != null)
        {
            var formId = Creature.Loader.GetId(lycanthropyForm.Name!);
            if (context.Effect.Duration == EffectDuration.Instant)
            {
                var marker = activeEffects.PropertyEffects.FirstOrDefault(
                    effect => effect.EffectType == EffectType.ConferLycanthropy
                            && effect.CombinationFunction == ValueCombinationFunction.Sum);
                var accumulator = activeEffects.Accumulators.GetValueOrDefault(EffectType.ConferLycanthropy);
                if (marker == null)
                {
                    activeEffects.AddPropertyEffect(
                        EffectType.ConferLycanthropy,
                        combinationFunction: ValueCombinationFunction.Sum,
                        magnitudeSecondary: formId);
                }
                else if (context.System.GetEffectiveStatusAmount(context) > (accumulator?.Value ?? 0))
                {
                    marker.MagnitudeSecondary = formId;
                }
            }

            context.System.ApplyStatus(context);

            return;
        }
    }

    public string CreatureName
    {
        get;
        set;
    } = null!;

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);
        effect.TargetName = CreatureName == PropagatedAmountPlaceholder
            ? null
            : CreatureName;
        SetSecondaryAmount(
            effect,
            CreatureName == PropagatedAmountPlaceholder
                ? secondaryAmountOverride ?? PropagatedAmountPlaceholder
                : null);
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ConferLycanthropy>(GetPropertyConditions<ConferLycanthropy>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(CreatureName), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
