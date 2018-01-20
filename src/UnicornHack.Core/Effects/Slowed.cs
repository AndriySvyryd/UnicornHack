using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Slowed : AppliedEffect
    {
        public Slowed()
        {
        }

        public Slowed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}