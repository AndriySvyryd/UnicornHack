using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class DamageEffect : Effect
    {
        public int Damage { get; set; }

        protected override void ConfigureEffect(EffectComponent effect) => effect.Amount = Damage;
    }
}
