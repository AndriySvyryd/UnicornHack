using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Blight : DamageEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Blight;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Blight>(GetPropertyConditions<Blight>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
