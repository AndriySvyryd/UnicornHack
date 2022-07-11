using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Cripple : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Cripple;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Cripple>(GetPropertyConditions<Cripple>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
