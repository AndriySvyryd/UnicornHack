using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Mute : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Mute, AbilityData.Muted.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.MuteResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "400*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Mute>(GetPropertyConditions<Mute>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
