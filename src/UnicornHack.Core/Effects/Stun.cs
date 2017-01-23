using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Stun : Effect
    {
        public Stun()
        {
        }

        public Stun(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Stun(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}