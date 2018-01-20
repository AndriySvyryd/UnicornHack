using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Sedated : AppliedEffect
    {
        public Sedated()
        {
        }

        public Sedated(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}