using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class LevelTeleport : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
        EffectType.LevelTeleport, defaultDuration: EffectDuration.Instant,
        combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<LevelTeleport>(GetPropertyConditions<LevelTeleport>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
