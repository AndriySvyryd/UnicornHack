using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class ElectricityDamage : DamageEffect
    {
        public ElectricityDamage()
        {
        }

        public ElectricityDamage(Game game)
            : base(game)
        {
        }

        // Shocks items
        // Removes slow
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new ElectricityDamage(game) {Damage = Damage};
    }
}