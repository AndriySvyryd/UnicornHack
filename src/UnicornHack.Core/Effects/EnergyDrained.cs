using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class EnergyDrained : AppliedEffect
    {
        public EnergyDrained()
        {
        }

        public EnergyDrained(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Amount { get; set; }
    }
}