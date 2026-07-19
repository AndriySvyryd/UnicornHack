using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Daze : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Daze, AbilityData.Dazed.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.DazeResistance),
            statusAbilityDuration: "100*FocusModifier()", defaultAmount: "200*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Daze>(GetPropertyConditions<Daze>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
