using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Slime : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Slime;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Slime>(GetPropertyConditions<Slime>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
