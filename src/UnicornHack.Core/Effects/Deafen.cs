using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Deafen : Effect
    {
        public Deafen()
        {
        }

        public Deafen(Game game) : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game) => new Deafen(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}