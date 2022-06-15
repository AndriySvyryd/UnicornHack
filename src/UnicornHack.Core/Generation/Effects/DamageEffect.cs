using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class DamageEffect : Effect
{
    private Func<GameEntity, GameEntity, float> _damageFunction;

    public string Damage
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        effect.Amount = Damage;

        if (Damage != null)
        {
            if (_damageFunction == null)
            {
                _damageFunction = EffectApplicationSystem.CreateAmountFunction(Damage, ContainingAbility.Name);
            }

            effect.AmountFunction = _damageFunction;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DamageEffect>(GetPropertyConditions<DamageEffect>());

    protected static new Dictionary<string, Func<TEffect, object, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = Effect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Damage), (o, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
