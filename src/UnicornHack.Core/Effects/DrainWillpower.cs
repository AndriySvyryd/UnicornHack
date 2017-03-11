using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainWillpower : Effect
    {
        public DrainWillpower()
        {
        }

        public DrainWillpower(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainWillpower(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}