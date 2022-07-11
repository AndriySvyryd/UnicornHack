using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Slow : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Slow;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Slow>(GetPropertyConditions<Slow>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
