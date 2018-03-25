using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Heal : Effect
    {
        public int Amount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.Heal;
            effect.Amount = Amount;
        }
    }
}
