using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class DrainEnergy : Effect
    {
        public string Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.DrainEnergy;
            effect.AmountExpression = Amount;
        }
    }
}
