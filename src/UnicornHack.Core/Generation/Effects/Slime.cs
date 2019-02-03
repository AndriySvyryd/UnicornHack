using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Slime : DurationEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Slime;
        }
    }
}
