using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stuck : AppliedEffect
    {
        public Stuck()
        {
        }

        public Stuck(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}