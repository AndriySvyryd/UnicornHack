using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public abstract class DamageEffect : Effect
    {
        protected DamageEffect()
        {
        }

        protected DamageEffect(Game game)
            : base(game)
        {
        }

        public int Damage { get; set; }

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Target.ChangeCurrentHP(-1 * Damage);
        }
    }
}