using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class StealGold : Effect
    {
        public StealGold()
        {
        }

        public StealGold(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new StealGold(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}