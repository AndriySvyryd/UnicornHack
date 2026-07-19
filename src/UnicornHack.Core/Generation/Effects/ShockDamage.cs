using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class ShockDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.ShockDamage, nameof(Systems.Beings.BeingComponent.ElectricityResistance),
        inflicts: [new(EffectType.Paralyze, 4, 0)],
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ShockDamage>(GetPropertyConditions<ShockDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
