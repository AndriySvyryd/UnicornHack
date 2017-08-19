using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class Polymorph : Effect
    {
        // Polymorphs items too
        public Polymorph()
        {
        }

        public Polymorph(Game game) : base(game)
        {
        }

        public override Effect Instantiate(Game game) => new Polymorph(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}