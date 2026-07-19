using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Enervate : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Enervate, AbilityData.Enervated.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.EnervateResistance),
            defaultDuration: EffectDuration.UntilXPGained,
            defaultDurationAmount: "2000*FocusModifier()", durationPolicy: DurationPolicy.Independent,
            combinationFunction: ValueCombinationFunction.Sum, defaultAmount: "16*FocusModifier()",
            propagatesAmountToStatusAbility: true);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Enervate>(GetPropertyConditions<Enervate>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
