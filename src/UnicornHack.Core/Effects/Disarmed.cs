using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Disarmed : AppliedEffect
    {
        public Disarmed()
        {
        }

        public Disarmed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}