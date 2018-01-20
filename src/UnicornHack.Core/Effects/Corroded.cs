using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Corroded : Damaged
    {
        public Corroded()
        {
        }

        public Corroded(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}