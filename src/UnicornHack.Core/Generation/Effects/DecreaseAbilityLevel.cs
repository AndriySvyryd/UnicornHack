using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class DecreaseAbilityLevel : Effect
{
    private static readonly EffectMetadata _metadata = EffectMetadata.Default(
        EffectType.DecreaseAbilityLevel,
        defaultDuration: EffectDuration.Infinite,
        combinationFunction: ValueCombinationFunction.Subtract,
        defaultAmount: "1",
        applicationHandler: Apply);

    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void RegisterMetadataProvider() => _ = _metadata;

    public AbilityType? AbilityType
    {
        get;
        set;
    }

    public string? AbilityName
    {
        get;
        set;
    }

    public override EffectMetadata Metadata => _metadata;

    internal static int EncodeAbilityTypeTarget(AbilityType abilityType)
        => -1 - (int)abilityType;

    internal static AbilityType? DecodeAbilityTypeTarget(int? targetEntityId)
        => targetEntityId is < 0
            ? (AbilityType)(-1 - targetEntityId.Value)
            : null;

    private static void Apply(in EffectApplicationContext context)
    {
        var amount = context.Amount;
        if (amount <= 0)
        {
            throw new InvalidOperationException(
                $"{nameof(DecreaseAbilityLevel)} amount must be positive, but was {amount}.");
        }

        context.System.ApplyDecreaseAbilityLevel(context, amount);
    }

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);

        if ((AbilityType == null) == (AbilityName == null))
        {
            throw new InvalidOperationException(
                $"{nameof(DecreaseAbilityLevel)} must set exactly one of {nameof(AbilityType)} and"
                + $" {nameof(AbilityName)}.");
        }

        effect.TargetEntityId = AbilityType is { } abilityType
            ? EncodeAbilityTypeTarget(abilityType)
            : null;
        effect.TargetName = AbilityName;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DecreaseAbilityLevel>(
            GetPropertyConditions<DecreaseAbilityLevel>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : DecreaseAbilityLevel
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();
        propertyConditions.Add(nameof(AbilityType), (_, value) => value != null);
        propertyConditions.Add(nameof(AbilityName), (_, value) => value != null);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}