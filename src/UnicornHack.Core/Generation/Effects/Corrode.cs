using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Corrode : DamageEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Corrode;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Corrode>(GetPropertyConditions<Corrode>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
