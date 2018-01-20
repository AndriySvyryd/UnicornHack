using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Crippled : AppliedEffect
    {
        public Crippled()
        {
        }

        public Crippled(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}