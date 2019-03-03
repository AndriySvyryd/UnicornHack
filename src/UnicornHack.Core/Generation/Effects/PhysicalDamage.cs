using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class PhysicalDamage : DamageEffect
    {
        public int ArmorPenetration { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.PhysicalDamage;
            effect.SecondaryAmount = ArmorPenetration;
        }
    }
}
