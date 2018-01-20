using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Teleported : AppliedEffect
    {
        public Teleported()
        {
        }

        public Teleported(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}