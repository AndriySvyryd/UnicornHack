using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Paralyze : Effect
    {
        public Paralyze()
        {
        }

        public Paralyze(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Paralyze(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}