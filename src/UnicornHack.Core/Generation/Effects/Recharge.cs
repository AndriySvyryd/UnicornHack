using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Recharge : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
            EffectType.Recharge, resourceProperty: nameof(Systems.Beings.BeingComponent.EnergyPoints),
            resistanceProperty: nameof(Systems.Beings.BeingComponent.EnergyRestorationResistance),
            defaultAmount: "10*FocusModifier()", combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Recharge>(GetPropertyConditions<Recharge>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
