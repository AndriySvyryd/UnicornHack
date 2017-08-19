using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Stick : Effect
    {
        public Stick()
        {
        }

        public Stick(Game game) : base(game)
        {
        }

        public override Effect Instantiate(Game game) => new Stick(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}