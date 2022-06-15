using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Stun : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Stun;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stun>(GetPropertyConditions<Stun>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
