using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Polymorph : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Polymorph,
            defaultDuration: EffectDuration.UntilTimeout,
            defaultDurationAmount: "500",
            combinationFunction: ValueCombinationFunction.Sum,
            applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public string CreatureName
    {
        get;
        set;
    } = null!;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
    {
        // TODO: Allow polymorphing items
        if (context.TargetEntity?.Being == null)
        {
            return;
        }

        if (!context.Pretend
            && FindPolymorphForm(context) is { } polymorphForm)
        {
            context.System.ApplyPolymorph(
                polymorphForm, context.TargetEntity,
                context.DurationAmount,
                context.Manager);
        }
    }

    public static Creature? FindPolymorphForm(in EffectApplicationContext context)
        => context.Effect.Component.TargetName is { } creatureName
            ? Creature.Loader.Find(creatureName)
            : context.SecondaryAmount is { } creatureId
                && creatureId != 0
                    ? Creature.Loader.FindById(creatureId)
                    : null;

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
        new PropertyCSScriptSerializer<Polymorph>(GetPropertyConditions<Polymorph>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(CreatureName), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
