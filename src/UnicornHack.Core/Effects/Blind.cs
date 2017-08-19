using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Blind : Effect
    {
        public Blind()
        {
        }

        public Blind(Game game) : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game) => new Blind(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}