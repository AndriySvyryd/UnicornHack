using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class FireDamage : DamageEffect
    {
        public FireDamage()
        {
        }

        public FireDamage(Game game)
            : base(game)
        {
        }

        // Burns items
        // Removes slime, wet, frozen
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new FireDamage(game) {Damage = Damage};
    }
}