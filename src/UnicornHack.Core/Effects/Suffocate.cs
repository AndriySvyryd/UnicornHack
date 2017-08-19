using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Suffocate : Effect
    {
        public Suffocate()
        {
        }

        public Suffocate(Game game) : base(game)
        {
        }

        public override Effect Instantiate(Game game) => new Suffocate(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}