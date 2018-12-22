using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class RemoveItem : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.RemoveItem;
    }
}
