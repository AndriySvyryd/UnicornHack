using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Confuse : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Confuse, AbilityData.Confused.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.DisorientResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "400*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Confuse>(GetPropertyConditions<Confuse>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
