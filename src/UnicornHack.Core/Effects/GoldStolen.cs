namespace UnicornHack.Effects
{
    public class GoldStolen : AppliedEffect
    {
        public GoldStolen()
        {
        }

        public GoldStolen(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Amount { get; set; }
    }
}