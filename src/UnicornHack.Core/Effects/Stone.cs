using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stone : DurationEffect
    {
        public Stone()
        {
        }

        public Stone(Game game) : base(game)
        {
        }

        public Stone(Stone effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Stone(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stoned(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}