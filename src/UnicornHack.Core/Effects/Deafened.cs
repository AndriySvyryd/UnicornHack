using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Deafened : AppliedEffect
    {
        public Deafened()
        {
        }

        public Deafened(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}