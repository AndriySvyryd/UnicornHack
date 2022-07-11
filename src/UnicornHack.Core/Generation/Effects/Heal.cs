using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Heal : AmountEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Heal;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Heal>(GetPropertyConditions<Heal>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
