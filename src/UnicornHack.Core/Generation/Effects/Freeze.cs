using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Freeze : DamageEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Freeze;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Freeze>(GetPropertyConditions<Freeze>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
