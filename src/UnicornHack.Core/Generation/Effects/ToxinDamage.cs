using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class ToxinDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.ToxinDamage, nameof(Systems.Beings.BeingComponent.ToxinResistance),
        statusAbilityName: AbilityData.Blighted.Name,
        defaultDurationAmount: "100*FocusModifier()", defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<ToxinDamage>(GetPropertyConditions<ToxinDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
