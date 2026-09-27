using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Cripple : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Cripple, AbilityData.Crippled.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.CrippleResistance),
            statusAbilityDuration: "200*MightModifier()", defaultAmount: "100*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Cripple>(GetPropertyConditions<Cripple>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
