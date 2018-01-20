using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Blinded : AppliedEffect
    {
        public Blinded()
        {
        }

        public Blinded(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}