using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class DrainLife : Effect
    {
        public string Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.DrainLife;
            effect.AmountExpression = Amount;
        }
    }
}
