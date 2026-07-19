using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Taunt : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Taunt, AbilityData.Taunted.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.TauntResistance),
            applicationHandler: Apply,
            statusAbilityDuration: "100*FocusModifier()",
            combinationFunction: ValueCombinationFunction.BuildUpPerSource,
            defaultAmount: "400*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
        => context.System.ApplyStatus(context);

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Taunt>(GetPropertyConditions<Taunt>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
