using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Slimed : AppliedEffect
    {
        public Slimed()
        {
        }

        public Slimed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}