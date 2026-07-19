using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class ResistedEffect : Effect
{
    public string? ResistancePenetration
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect, string? secondaryAmountOverride)
    {
        base.ConfigureEffect(effect, secondaryAmountOverride);
        SetSecondaryAmount(
            effect,
            ResistancePenetration == PropagatedAmountPlaceholder
                ? secondaryAmountOverride ?? PropagatedAmountPlaceholder
                : ResistancePenetration);
    }

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : ResistedEffect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();
        propertyConditions.Add(nameof(ResistancePenetration), (_, value) => value != null);
        return propertyConditions;
    }
}