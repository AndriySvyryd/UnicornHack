namespace UnicornHack.Effects
{
    public class LifeDrained : AppliedEffect
    {
        public LifeDrained()
        {
        }

        public LifeDrained(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Amount { get; set; }
    }
}