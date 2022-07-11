using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Bind : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Bind;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Bind>(GetPropertyConditions<Bind>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
