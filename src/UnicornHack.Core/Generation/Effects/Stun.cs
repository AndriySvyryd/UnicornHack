using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Stun : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Stun, AbilityData.Stunned.Name,
            statusCategory: StatusCategory.Physical,
            statusAbilityDuration: "100*MightModifier()",
            defaultAmount: "200*MightModifier()", durationPolicy: DurationPolicy.Extend,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.StunResistance));

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stun>(GetPropertyConditions<Stun>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
