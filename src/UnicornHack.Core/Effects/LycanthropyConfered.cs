namespace UnicornHack.Effects
{
    public class LycanthropyConfered : AppliedEffect
    {
        public LycanthropyConfered()
        {
        }

        public LycanthropyConfered(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public string VariantName { get; set; }
    }
}