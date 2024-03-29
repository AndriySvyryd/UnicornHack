﻿using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class DamageEffect : Effect
{
    private Func<GameEntity, GameEntity, float>? _damageFunction;

    public string Damage
    {
        get;
        set;
    } = null!;

    protected override void ConfigureEffect(EffectComponent effect)
    {
        effect.Amount = Damage;

        if (_damageFunction == null)
        {
            _damageFunction = EffectApplicationSystem.CreateAmountFunction(Damage, ContainingAbility.Name);
        }

        effect.AmountFunction = _damageFunction;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DamageEffect>(GetPropertyConditions<DamageEffect>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Damage), (_, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
