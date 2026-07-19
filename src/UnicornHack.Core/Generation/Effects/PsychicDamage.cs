using CSharpScriptSerialization;

namespace UnicornHack.Generation.Effects;

public class PsychicDamage : ResistedEffect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
        EffectType.PsychicDamage, nameof(Systems.Beings.BeingComponent.PsychicResistance),
        inflicts: [new(EffectType.Confuse, 8, 0)],
        defaultAmount: "10*FocusModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public override EffectMetadata Metadata => _metadata;

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<PsychicDamage>(GetPropertyConditions<PsychicDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
