using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Confused : AppliedEffect
    {
        public Confused()
        {
        }

        public Confused(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}