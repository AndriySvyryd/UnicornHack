using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Paralyzed : AppliedEffect
    {
        public Paralyzed()
        {
        }

        public Paralyzed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}