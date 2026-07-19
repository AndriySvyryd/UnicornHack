using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class DisableItem : Effect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.DisableItem, defaultDuration: EffectDuration.Instant,
            combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DisableItem>(GetPropertyConditions<DisableItem>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
