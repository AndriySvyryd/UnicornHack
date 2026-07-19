using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Expose : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Expose, AbilityData.Exposed.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.ExposeResistance),
            statusAbilityDuration: "100*MightModifier()", defaultAmount: "600*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Expose>(GetPropertyConditions<Expose>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
