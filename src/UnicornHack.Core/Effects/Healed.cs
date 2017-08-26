namespace UnicornHack.Effects
{
    public class Healed : AppliedEffect
    {
        public Healed()
        {
        }

        public Healed(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Amount { get; set; }
    }
}