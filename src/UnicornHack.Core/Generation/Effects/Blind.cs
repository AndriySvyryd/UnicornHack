using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Blind : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Blind;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Blind>(GetPropertyConditions<Blind>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
