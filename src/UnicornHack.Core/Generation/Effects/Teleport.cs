using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Teleport : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.Teleport;
    }
}
