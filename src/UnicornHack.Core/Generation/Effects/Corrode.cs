using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Corrode : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Corrode, AbilityData.Corroded.Name,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.CorrodeResistance),
            statusAbilityDuration: "100*MightModifier()", defaultAmount: "1000*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Corrode>(GetPropertyConditions<Corrode>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
