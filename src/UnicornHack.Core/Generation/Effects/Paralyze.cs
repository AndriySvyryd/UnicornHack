using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Paralyze : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Paralyze, AbilityData.Paralyzed.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.ParalyzeResistance),
            statusAbilityDuration: "100*FocusModifier()",
            defaultAmount: "200*FocusModifier()", durationPolicy: DurationPolicy.Extend);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Paralyze>(GetPropertyConditions<Paralyze>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
