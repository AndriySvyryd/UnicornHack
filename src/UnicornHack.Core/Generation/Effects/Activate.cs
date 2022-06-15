using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Activate : DurationEffect
{
    public string Projectile
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Activate;

        if (Projectile != null)
        {
            var item = Item.Loader.Get(Projectile).AddToEntity(effect.Entity);
            effect.TargetEntityId = item.EntityId;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Activate>(GetPropertyConditions<Activate>());

    protected static new Dictionary<string, Func<TEffect, object, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = DurationEffect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(Projectile), (o, v) => v != default);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
