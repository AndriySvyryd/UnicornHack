using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class LycanthropyConfered : AppliedEffect
    {
        public LycanthropyConfered()
        {
        }

        public LycanthropyConfered(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public string VariantName { get; set; }
    }
}