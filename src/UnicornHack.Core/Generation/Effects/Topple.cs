using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Topple : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Topple, AbilityData.Unstable.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.ToppleResistance),
            statusAbilityDuration: "100*MightModifier()",
            defaultAmount: "200*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Topple>(GetPropertyConditions<Topple>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
