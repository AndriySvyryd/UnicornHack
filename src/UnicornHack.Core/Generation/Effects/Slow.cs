using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Slow : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Slow, AbilityData.Slowed.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.SlowResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "600*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Slow>(GetPropertyConditions<Slow>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
