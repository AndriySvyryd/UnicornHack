using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Suffocate : ResistedEffect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Suffocate, defaultDuration: EffectDuration.Instant,
            statusCategory: StatusCategory.Physical,
            resistanceProperty: nameof(Systems.Beings.BeingComponent.SuffocateResistance),
            combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Suffocate>(GetPropertyConditions<Suffocate>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
