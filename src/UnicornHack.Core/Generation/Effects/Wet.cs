using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Wet : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Wet, AbilityData.Wetted.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.WetResistance),
            statusAbilityDuration: "100*MightModifier()",
            defaultAmount: "600*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Wet>(GetPropertyConditions<Wet>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
