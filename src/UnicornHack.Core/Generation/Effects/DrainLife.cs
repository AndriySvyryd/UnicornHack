using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class DrainLife : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
            EffectType.DrainLife, nameof(Systems.Beings.BeingComponent.BleedingResistance),
            resourceProperty: nameof(Systems.Beings.BeingComponent.HitPoints),
            restoresActivator: true,
            defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DrainLife>(GetPropertyConditions<DrainLife>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
