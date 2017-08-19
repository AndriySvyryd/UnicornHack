using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Slow : Effect
    {
        public Slow()
        {
        }

        public Slow(Game game) : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game) => new Slow(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}