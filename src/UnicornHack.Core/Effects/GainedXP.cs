using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class GainedXP : AppliedEffect
    {
        public GainedXP()
        {
        }

        public GainedXP(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Amount { get; set; }
    }
}