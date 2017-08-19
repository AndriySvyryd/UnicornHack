using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class VenomDamage : DamageEffect
    {
        public VenomDamage()
        {
        }

        public VenomDamage(Game game) : base(game)
        {
        }

        // Decays items, gives temporary venom damage bonus
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game) => new VenomDamage(game) {Damage = Damage};
    }
}