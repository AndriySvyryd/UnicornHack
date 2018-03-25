using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Shock : DamageEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Shock;
        }
    }
}
