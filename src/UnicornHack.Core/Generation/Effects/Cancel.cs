using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class Cancel : Effect
{
    private static readonly EffectMetadata _metadata =
        EffectMetadata.Default(
            EffectType.Cancel, AbilityData.Canceled.Name,
            defaultDuration: EffectDuration.UntilTimeout, defaultDurationAmount: "300*FocusModifier()",
            combinationFunction: ValueCombinationFunction.Sum);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Cancel>(GetPropertyConditions<Cancel>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
