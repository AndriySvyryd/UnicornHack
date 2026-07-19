using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class FireDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.FireDamage, nameof(Systems.Beings.BeingComponent.FireResistance),
        statusAbilityName: AbilityData.Burning.Name,
        defaultDurationAmount: "100*FocusModifier()",
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<FireDamage>(GetPropertyConditions<FireDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
