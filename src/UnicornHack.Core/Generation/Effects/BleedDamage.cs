using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class BleedDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.BleedDamage, nameof(Systems.Beings.BeingComponent.BleedingResistance),
        statusAbilityName: AbilityData.Drained.Name,
        extendsOnMovement: true,
        defaultDurationAmount: "100*MightModifier()", defaultAmount: "10*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<BleedDamage>(GetPropertyConditions<BleedDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
