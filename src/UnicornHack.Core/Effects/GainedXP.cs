using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class GainedXP : AppliedEffect
    {
        public GainedXP()
        {
        }

        public GainedXP(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Amount { get; set; }
    }
}