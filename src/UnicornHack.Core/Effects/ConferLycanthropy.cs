namespace UnicornHack.Effects
{
    public class ConferLycanthropy : Effect
    {
        public ConferLycanthropy()
        {
        }

        public ConferLycanthropy(Game game) : base(game)
        {
        }

        public string VariantName { get; set; }

        public override Effect Copy(Game game) => new ConferLycanthropy(game) {VariantName = VariantName};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new LycanthropyConfered(abilityContext) {VariantName = VariantName});
        }
    }
}