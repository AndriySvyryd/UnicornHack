using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Burned : Damaged
    {
        public Burned()
        {
        }

        public Burned(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}