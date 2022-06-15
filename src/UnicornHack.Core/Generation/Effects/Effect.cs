using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public abstract class Effect : ICSScriptSerializable
{
    public Ability ContainingAbility
    {
        get;
        set;
    }

    public bool TargetActivator
    {
        get;
        set;
    }

    public EffectComponent AddToAbility(AbilityComponent ability, GameManager manager)
    {
        var continuous = (ability.Activation & ActivationType.Continuous) != 0;
        using (var effectEntityReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.ContainingAbilityId = ability.EntityId;
            effect.ShouldTargetActivator = TargetActivator;

            effectEntityReference.Referenced.Effect = effect;

            ConfigureEffect(effect);

            if (continuous
                && effect.Duration == EffectDuration.Instant)
            {
                effect.Duration = EffectDuration.Infinite;
            }

            return effect;
        }
    }

    protected abstract void ConfigureEffect(EffectComponent effect);

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Effect>(GetPropertyConditions<Effect>());

    protected static Dictionary<string, Func<TEffect, object, bool>> GetPropertyConditions<TEffect>()
        where TEffect : Effect => new()
    {
        { nameof(ContainingAbility), (o, v) => false }, { nameof(TargetActivator), (o, v) => (bool)v },
    };

    public virtual ICSScriptSerializer GetSerializer() => Serializer;
}
