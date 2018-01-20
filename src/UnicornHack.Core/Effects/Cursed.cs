using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Cursed : AppliedEffect
    {
        public Cursed()
        {
        }

        public Cursed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}