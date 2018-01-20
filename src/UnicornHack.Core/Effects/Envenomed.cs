using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Envenomed : Damaged
    {
        public Envenomed()
        {
        }

        public Envenomed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}