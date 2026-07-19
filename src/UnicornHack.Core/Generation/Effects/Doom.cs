using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Doom : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Doom, statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.DoomResistance),
            combinationFunction: ValueCombinationFunction.BuildUpHealth,
            defaultDuration: EffectDuration.UntilTimeout,
            durationPolicy: DurationPolicy.Extend, defaultAmount: "20*FocusModifier()",
            buildUpRealizationHandler: Realize);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Realize(in BuildUpRealizationContext context)
    {
        if (context.System.RealizeBuildUp(context, decay: 1)
            && context.Accumulator.Value > EffectApplicationSystem.GetBuildUpThreshold(
                ValueCombinationFunction.BuildUpHealth, context.ActorEntity))
        {
            context.ActorEntity.Being!.HitPoints = 0;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Doom>(GetPropertyConditions<Doom>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
