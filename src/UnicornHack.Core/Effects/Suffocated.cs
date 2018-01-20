using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Suffocated : AppliedEffect
    {
        public Suffocated()
        {
        }

        public Suffocated(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}