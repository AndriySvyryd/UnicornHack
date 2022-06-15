using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class DrainEnergy : AmountEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.DrainEnergy;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<DrainEnergy>(GetPropertyConditions<DrainEnergy>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
