using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stick : DurationEffect
    {
        public Stick()
        {
        }

        public Stick(Game game) : base(game)
        {
        }

        public Stick(Stick effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Stick(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stuck(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}