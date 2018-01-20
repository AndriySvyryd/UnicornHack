using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class LifeDrained : AppliedEffect
    {
        public LifeDrained()
        {
        }

        public LifeDrained(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Amount { get; set; }
    }
}