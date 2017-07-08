using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class ColdDamage : DamageEffect
    {
        public ColdDamage()
        {
        }

        public ColdDamage(Game game)
            : base(game)
        {
        }

        // Freezes items
        // Removes burning, dissolving
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new ColdDamage(game) {Damage = Damage};
    }
}