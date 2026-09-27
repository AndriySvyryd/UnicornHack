using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Weaken : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Weaken, AbilityData.Weakened.Name,
            statusCategory: StatusCategory.Physical,
            statusAbilityDuration: "100*MightModifier()", defaultAmount: "200*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Weaken>(GetPropertyConditions<Weaken>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
