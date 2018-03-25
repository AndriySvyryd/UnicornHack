using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class StealItem : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.StealItem;
    }
}
