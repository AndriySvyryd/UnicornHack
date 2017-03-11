using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Hallucinate : Effect
    {
        public Hallucinate()
        {
        }

        public Hallucinate(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Hallucinate(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}