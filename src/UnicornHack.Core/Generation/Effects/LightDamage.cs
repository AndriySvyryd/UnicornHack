using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class LightDamage : DamageEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.LightDamage;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<LightDamage>(GetPropertyConditions<LightDamage>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
