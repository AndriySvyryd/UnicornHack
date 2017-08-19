using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Disintegrate : Effect
    {
        public Disintegrate()
        {
        }

        public Disintegrate(Game game) : base(game)
        {
        }

        // Withers items
        public int Damage { get; set; }

        public override Effect Instantiate(Game game) => new Disintegrate(game) {Damage = Damage};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}