using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Engulfed : AppliedEffect
    {
        public Engulfed()
        {
        }

        public Engulfed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}