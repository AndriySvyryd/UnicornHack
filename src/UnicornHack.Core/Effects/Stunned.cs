using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stunned : AppliedEffect
    {
        public Stunned()
        {
        }

        public Stunned(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}