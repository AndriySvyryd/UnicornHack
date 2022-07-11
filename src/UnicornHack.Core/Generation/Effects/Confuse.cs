using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Confuse : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Confuse;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Confuse>(GetPropertyConditions<Confuse>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
