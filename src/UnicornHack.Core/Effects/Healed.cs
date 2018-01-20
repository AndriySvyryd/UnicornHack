using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Healed : AppliedEffect
    {
        public Healed()
        {
        }

        public Healed(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Amount { get; set; }
    }
}