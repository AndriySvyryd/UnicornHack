using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Disarm : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.Disarm;
    }
}
