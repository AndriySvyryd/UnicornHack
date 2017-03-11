using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Stone : Effect
    {
        public Stone()
        {
        }

        public Stone(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Stone(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}