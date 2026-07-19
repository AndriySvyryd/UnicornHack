using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Overcharge : Effect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Overcharge, combinationFunction: ValueCombinationFunction.BuildUpFixed,
            defaultDuration: EffectDuration.UntilTimeout,
            durationPolicy: DurationPolicy.Extend, defaultAmount: "10*FocusModifier()",
            buildUpRealizationHandler: Realize);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Realize(in BuildUpRealizationContext context)
    {
        if (!context.System.RealizeBuildUp(context, decay: 0)
            || context.Accumulator.Value < EffectApplicationSystem.GetBuildUpThreshold(
                ValueCombinationFunction.BuildUpFixed, context.ActorEntity))
        {
            return;
        }

        var being = context.ActorEntity.Being!;
        var discharge = being.EnergyPoints;
        being.EnergyPoints = 0;
        if (discharge > 0)
        {
            being.HitPoints -= discharge;
        }

        context.ActorEntity.ActiveEffects!.Accumulators.Remove(context.Accumulator);
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Overcharge>(GetPropertyConditions<Overcharge>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
