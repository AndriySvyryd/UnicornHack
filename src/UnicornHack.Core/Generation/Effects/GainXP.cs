using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class GainXP : Effect
    {
        public int Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.GainXP;
            effect.Amount = Amount;
        }
    }
}
