using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainLife : Effect
    {
        public DrainLife()
        {
        }

        public DrainLife(Game game) : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game) => new DrainLife(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}