using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Frozen : Damaged
    {
        public Frozen()
        {
        }

        public Frozen(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}