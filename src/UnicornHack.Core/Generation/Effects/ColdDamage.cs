using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class ColdDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.ColdDamage, nameof(Systems.Beings.BeingComponent.ColdResistance),
        inflicts: [new(EffectType.Chill, 10, 0)],
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ColdDamage>(GetPropertyConditions<ColdDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
