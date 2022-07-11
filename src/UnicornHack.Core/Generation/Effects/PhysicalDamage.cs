using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class PhysicalDamage : DamageEffect
{
    private Func<GameEntity, GameEntity, float>? _armorPenetrationFunction;

    public string? ArmorPenetration
    {
        get;
        set;
    }

    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.PhysicalDamage;
        effect.SecondaryAmount = ArmorPenetration;

        if (!string.IsNullOrEmpty(ArmorPenetration))
        {
            if (_armorPenetrationFunction == null)
            {
                _armorPenetrationFunction =
                    EffectApplicationSystem.CreateAmountFunction(ArmorPenetration, ContainingAbility.Name);
            }

            effect.SecondaryAmountFunction = _armorPenetrationFunction;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<PhysicalDamage>(GetPropertyConditions<PhysicalDamage>());

    protected static new Dictionary<string, Func<TEffect, object?, bool>>
        GetPropertyConditions<TEffect>() where TEffect : Effect
    {
        var propertyConditions = DamageEffect.GetPropertyConditions<TEffect>();

        propertyConditions.Add(nameof(ArmorPenetration), (_, v) => v != null);
        return propertyConditions;
    }

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
