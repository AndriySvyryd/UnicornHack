using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Disarm : Effect
    {
        public Disarm()
        {
        }

        public Disarm(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Disarm(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}