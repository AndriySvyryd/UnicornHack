using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects;

public class Suffocate : DurationEffect
{
    protected override void ConfigureEffect(EffectComponent effect)
    {
        base.ConfigureEffect(effect);

        effect.EffectType = EffectType.Suffocate;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Suffocate>(GetPropertyConditions<Suffocate>());

    public override ICSScriptSerializer GetSerializer() => Serializer;
}
