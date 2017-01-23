using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Engulf : Effect
    {
        public Engulf()
        {
        }

        public Engulf(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Engulf(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}