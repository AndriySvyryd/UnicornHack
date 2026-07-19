using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class AcidDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Resource(
            EffectType.AcidDamage, nameof(Systems.Beings.BeingComponent.AcidResistance),
            defaultAmount: "10*FocusModifier()", inflicts: [new(EffectType.Corrode, 100, 0)]);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<AcidDamage>(GetPropertyConditions<AcidDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
