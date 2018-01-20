using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Shocked : Damaged
    {
        public Shocked()
        {
        }

        public Shocked(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}