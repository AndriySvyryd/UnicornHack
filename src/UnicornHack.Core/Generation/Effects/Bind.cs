using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Bind : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Bind, AbilityData.Bound.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.BindResistance),
            statusAbilityDuration: "100*MightModifier()",
            defaultAmount: "200*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Bind>(GetPropertyConditions<Bind>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
