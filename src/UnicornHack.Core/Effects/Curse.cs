using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Curse : Effect
    {
        public Curse()
        {
        }

        public Curse(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Curse(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}