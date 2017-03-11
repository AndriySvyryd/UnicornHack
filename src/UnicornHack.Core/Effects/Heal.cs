using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Heal : Effect
    {
        public Heal()
        {
        }

        public Heal(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new Heal(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}