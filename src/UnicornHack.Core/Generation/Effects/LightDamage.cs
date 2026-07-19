using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class LightDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Resource(
            EffectType.LightDamage, nameof(Systems.Beings.BeingComponent.LightResistance),
            inflicts: [new(EffectType.Blind, 4, 0)], defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<LightDamage>(GetPropertyConditions<LightDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
