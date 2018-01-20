using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class GoldStolen : AppliedEffect
    {
        public GoldStolen()
        {
        }

        public GoldStolen(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Amount { get; set; }
    }
}