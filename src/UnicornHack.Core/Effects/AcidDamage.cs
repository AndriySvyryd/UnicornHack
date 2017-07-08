using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class AcidDamage : DamageEffect
    {
        public AcidDamage()
        {
        }

        public AcidDamage(Game game)
            : base(game)
        {
        }

        // Corrodes items
        // Removes stoning
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new AcidDamage(game) {Damage = Damage};
    }
}