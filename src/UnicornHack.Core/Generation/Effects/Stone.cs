using CSharpScriptSerialization;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Stone : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Stone;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Stone>(GetPropertyConditions<Stone>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
