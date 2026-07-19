using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Stick : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
            EffectType.Stick, AbilityData.Stuck.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.StickResistance),
            statusAbilityDuration: "100*MightModifier()",
            defaultAmount: "400*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stick>(GetPropertyConditions<Stick>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
