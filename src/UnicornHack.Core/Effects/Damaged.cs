using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Damaged : AppliedEffect
    {
        public Damaged()
        {
        }

        public Damaged(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Damage { get; set; }
    }
}