using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class LevelTeleport : Effect
    {
        public LevelTeleport()
        {
        }

        public LevelTeleport(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new LevelTeleport(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}