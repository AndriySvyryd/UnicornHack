using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class MagicalDamage : DamageEffect
    {
        public MagicalDamage()
        {
        }

        public MagicalDamage(Game game)
            : base(game)
        {
        }

        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.Ability.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game)
            => new MagicalDamage(game) {Damage = Damage};
    }
}