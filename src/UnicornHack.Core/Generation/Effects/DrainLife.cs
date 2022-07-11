using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class DrainLife : AmountEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.DrainLife;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DrainLife>(GetPropertyConditions<DrainLife>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
