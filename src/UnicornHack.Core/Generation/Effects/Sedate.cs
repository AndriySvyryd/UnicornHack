using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Sedate : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Sedate, AbilityData.Sleepy.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.SedateResistance),
            statusAbilityDuration: "100*FocusModifier()",
            defaultAmount: "200*FocusModifier()", durationPolicy: DurationPolicy.Extend);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Sedate>(GetPropertyConditions<Sedate>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
