using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Decay : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Decay, AbilityData.Decayed.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.DecayResistance),
            defaultDuration: EffectDuration.UntilXPGained,
            defaultDurationAmount: "2000*MightModifier()", durationPolicy: DurationPolicy.Independent,
            combinationFunction: ValueCombinationFunction.Sum, defaultAmount: "20*MightModifier()",
            propagatesAmountToStatusAbility: true);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Decay>(GetPropertyConditions<Decay>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
