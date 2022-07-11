using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class AmountEffect : Effect
{
    private Func<GameEntity, GameEntity, float>? _amountFunction;

    public string Amount
    {
        get;
        set;
    } = null!;

    protected override void ConfigureEffect(EffectComponent effect)
    {
        effect.Amount = Amount;

        if (_amountFunction == null)
        {
            _amountFunction = EffectApplicationSystem.CreateAmountFunction(Amount, ContainingAbility.Name);
        }

        effect.AmountFunction = _amountFunction;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<AmountEffect>(GetPropertyConditions<AmountEffect>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Amount), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
