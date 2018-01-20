using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Sedate : DurationEffect
    {
        public Sedate()
        {
        }

        public Sedate(Game game) : base(game)
        {
        }

        public Sedate(Sedate effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Sedate(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Sedated(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}