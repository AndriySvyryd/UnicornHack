using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class LevelTeleport : Effect
    {
        public LevelTeleport()
        {
        }

        public LevelTeleport(Game game) : base(game)
        {
        }

        public LevelTeleport(LevelTeleport effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new LevelTeleport(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new LevelTeleported(abilityContext, TargetActivator));
        }
    }
}