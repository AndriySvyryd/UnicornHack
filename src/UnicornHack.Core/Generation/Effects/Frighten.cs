using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Frighten : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Frighten, AbilityData.Frightened.Name,
            statusCategory: StatusCategory.Mental,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.FrightenResistance),
            applicationHandler: Apply,
            statusAbilityDuration: "100*FocusModifier()",
            combinationFunction: ValueCombinationFunction.BuildUpPerSource,
            defaultAmount: "600*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static void Apply(in EffectApplicationContext context)
        => context.System.ApplyStatus(context);

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Frighten>(GetPropertyConditions<Frighten>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
