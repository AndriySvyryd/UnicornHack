using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Fascinate : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Fascinate, AbilityData.Fascinated.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.FascinateResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "200*FocusModifier()",
            durationPolicy: DurationPolicy.Extend);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Fascinate>(GetPropertyConditions<Fascinate>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
