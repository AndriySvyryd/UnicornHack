using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Paralyze : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Paralyze;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Paralyze>(GetPropertyConditions<Paralyze>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
