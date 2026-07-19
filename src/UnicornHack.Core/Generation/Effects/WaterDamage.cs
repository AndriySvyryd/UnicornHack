using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class WaterDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.WaterDamage, nameof(Systems.Beings.BeingComponent.WaterResistance),
        inflicts: [new(EffectType.Wet, 14, 0)],
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<WaterDamage>(GetPropertyConditions<WaterDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
