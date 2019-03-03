using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Recharge : Effect
    {
        public int Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.Recharge;
            effect.Amount = Amount;
        }
    }
}
