using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class DrainEnergy : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
            EffectType.DrainEnergy, nameof(Systems.Beings.BeingComponent.VoidResistance),
            resourceProperty: nameof(Systems.Beings.BeingComponent.EnergyPoints),
            restoresActivator: true,
            defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DrainEnergy>(GetPropertyConditions<DrainEnergy>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
