using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Teleport : Effect
    {
        public Teleport()
        {
        }

        public Teleport(Game game) : base(game)
        {
        }

        public Teleport(Teleport effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Teleport(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Teleported(abilityContext, TargetActivator));
        }
    }
}