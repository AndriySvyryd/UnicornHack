using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Slime : Effect
    {
        public Slime()
        {
        }

        public Slime(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Slime(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}