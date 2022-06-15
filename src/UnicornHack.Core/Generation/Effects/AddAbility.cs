using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class AddAbility : DurationEffect
{
    public Ability Ability
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public int Level
    {
        get;
        set;
    }

    public ValueCombinationFunction Function
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.AddAbility;

        if (Ability != null)
        {
            Ability.AddToEffect(effect.Entity);
        }
        else
        {
            effect.TargetName = Name;
            effect.AppliedAmount = Level;
            effect.CombinationFunction = Function;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<AddAbility>(GetPropertyConditions<AddAbility>());

    protected static new Dictionary<string, Func<TEffect, object, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = DurationEffect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Ability), (o, v) => v != default);
        propertyConditions.Add(nameof(Name), (o, v) => v != default);
        propertyConditions.Add(nameof(Level), (o, v) => (int)v != default);
        propertyConditions.Add(nameof(Function), (o, v) => (ValueCombinationFunction)v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
