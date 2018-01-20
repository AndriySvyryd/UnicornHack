using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Soaked : Damaged
    {
        public Soaked()
        {
        }

        public Soaked(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}