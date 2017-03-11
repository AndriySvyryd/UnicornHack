using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainStrength : Effect
    {
        public DrainStrength()
        {
        }

        public DrainStrength(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainStrength(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}