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

        public override Effect Copy(Game game) => new LevelTeleport(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new LevelTeleported(abilityContext));
        }
    }
}