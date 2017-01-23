using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Bind : Effect
    {
        public Bind()
        {
        }

        public Bind(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Bind(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}