using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class GainXP : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.GainXP, resourceProperty: nameof(Systems.Beings.BeingComponent.ExperiencePoints),
        combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<GainXP>(GetPropertyConditions<GainXP>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
