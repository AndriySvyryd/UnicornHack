using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class PoisonDamage : DamageEffect
    {
        public PoisonDamage()
        {
        }

        public PoisonDamage(Game game)
            : base(game)
        {
        }

        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new PoisonDamage(game) {Damage = Damage};
    }
}