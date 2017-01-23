using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainIntelligence : Effect
    {
        public DrainIntelligence()
        {
        }

        public DrainIntelligence(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainIntelligence(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}