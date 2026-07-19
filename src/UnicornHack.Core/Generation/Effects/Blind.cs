using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Blind : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Blind, AbilityData.Blinded.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.BlindResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "400*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Blind>(GetPropertyConditions<Blind>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
