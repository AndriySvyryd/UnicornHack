using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class SonicDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Resource(
            EffectType.SonicDamage, nameof(Systems.Beings.BeingComponent.SonicResistance),
            inflicts: [new(EffectType.Daze, 8, 0)], defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<SonicDamage>(GetPropertyConditions<SonicDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
