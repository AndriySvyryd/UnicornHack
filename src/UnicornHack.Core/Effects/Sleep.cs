using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Sleep : Effect
    {
        public Sleep()
        {
        }

        public Sleep(Game game) : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game) => new Sleep(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}