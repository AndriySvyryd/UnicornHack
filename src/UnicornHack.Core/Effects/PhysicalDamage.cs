using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class PhysicalDamage : DamageEffect
    {
        public PhysicalDamage()
        {
        }

        public PhysicalDamage(Game game)
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
            => new PhysicalDamage(game) {Damage = Damage};
    }
}