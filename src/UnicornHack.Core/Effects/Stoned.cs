using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stoned : AppliedEffect
    {
        public Stoned()
        {
        }

        public Stoned(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}