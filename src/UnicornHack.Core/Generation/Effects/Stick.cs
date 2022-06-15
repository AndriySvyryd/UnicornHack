using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Stick : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Stick;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stick>(GetPropertyConditions<Stick>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
