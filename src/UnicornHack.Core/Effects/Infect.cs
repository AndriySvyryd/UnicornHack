using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Infect : Effect
    {
        public Infect()
        {
        }

        public Infect(Game game)
            : base(game)
        {
        }

        public int Strength { get; set; }

        public override Effect Instantiate(Game game)
            => new Infect(game) {Strength = Strength};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}