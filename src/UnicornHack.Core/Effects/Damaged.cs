using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public abstract class Damaged : AppliedEffect
    {
        protected Damaged()
        {
        }

        protected Damaged(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Damage { get; set; }
    }
}