using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class PhysicalDamage : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Resource(
            EffectType.PhysicalDamage, nameof(Systems.Beings.BeingComponent.PhysicalResistance),
            absorptionProperty: nameof(Systems.Beings.BeingComponent.Armor), includesMagicResistance: false,
            defaultAmount: "10*MightModifier()");

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public string? ArmorPenetration
    {
        get;
        set;
    }

    public override EffectMetadata Metadata => _metadata;

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);
        SetSecondaryAmount(
            effect,
            ArmorPenetration == PropagatedAmountPlaceholder
                ? secondaryAmountOverride ?? PropagatedAmountPlaceholder
                : ArmorPenetration);
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<PhysicalDamage>(GetPropertyConditions<PhysicalDamage>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(ArmorPenetration), (_, v) => v != null);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
