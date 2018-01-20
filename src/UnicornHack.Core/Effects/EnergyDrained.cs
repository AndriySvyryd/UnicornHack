using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class EnergyDrained : AppliedEffect
    {
        public EnergyDrained()
        {
        }

        public EnergyDrained(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Amount { get; set; }
    }
}