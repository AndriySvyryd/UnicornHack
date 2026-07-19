using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Chill : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Chill, AbilityData.Chilled.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.ColdResistance),
            statusAbilityDuration: "100*MightModifier()", defaultAmount: "600*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Chill>(GetPropertyConditions<Chill>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
