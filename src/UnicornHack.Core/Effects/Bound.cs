using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Bound : AppliedEffect
    {
        public Bound()
        {
        }

        public Bound(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}