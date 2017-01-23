using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class ConferLycanthropy : Effect
    {
        public ConferLycanthropy()
        {
        }

        public ConferLycanthropy(Game game)
            : base(game)
        {
        }

        public string VariantName { get; set; }

        public override Effect Instantiate(Game game)
            => new ConferLycanthropy(game) {VariantName = VariantName};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}