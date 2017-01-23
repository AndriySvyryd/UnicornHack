using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainSpeed : Effect
    {
        public DrainSpeed()
        {
        }

        public DrainSpeed(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainSpeed(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}