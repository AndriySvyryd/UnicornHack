using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Stagger : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Stagger, AbilityData.Staggered.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.StaggerResistance),
            statusAbilityDuration: "100*MightModifier()", defaultAmount: "400*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stagger>(GetPropertyConditions<Stagger>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
