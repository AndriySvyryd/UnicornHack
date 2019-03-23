using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class DamageEffect : Effect
    {
        public string Damage { get; set; }

        protected override void ConfigureEffect(EffectComponent effect) => effect.AmountExpression = Damage;
    }
}
