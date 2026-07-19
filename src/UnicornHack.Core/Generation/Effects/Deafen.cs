using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Deafen : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Deafen, AbilityData.Deafened.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.DeafenResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "600*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Deafen>(GetPropertyConditions<Deafen>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
