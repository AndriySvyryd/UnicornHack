using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Heal : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
            EffectType.Heal, resourceProperty: nameof(Systems.Beings.BeingComponent.HitPoints),
            resistanceProperty: nameof(Systems.Beings.BeingComponent.HealingResistance),
            defaultAmount: "10*FocusModifier()", combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Heal>(GetPropertyConditions<Heal>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
