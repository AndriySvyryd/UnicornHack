using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Wither : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.Wither, nameof(Systems.Beings.BeingComponent.VoidResistance),
        resourceProperty: nameof(Systems.Beings.BeingComponent.EnergyPoints),
        statusAbilityName: AbilityData.Withered.Name,
        defaultDurationAmount: "100*FocusModifier()",
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Wither>(GetPropertyConditions<Wither>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
