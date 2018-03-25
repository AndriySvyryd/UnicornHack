using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class StealGold : Effect
    {
        public int Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.StealGold;
            effect.Amount = Amount;
        }
    }
}
