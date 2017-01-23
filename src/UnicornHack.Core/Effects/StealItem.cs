using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class StealItem : Effect
    {
        public StealItem()
        {
        }

        public StealItem(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new StealItem(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}