using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Cripple : Effect
    {
        public Cripple()
        {
        }

        public Cripple(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Cripple(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}