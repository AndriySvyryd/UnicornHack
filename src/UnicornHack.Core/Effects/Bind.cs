using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Bind : DurationEffect
    {
        public Bind()
        {
        }

        public Bind(Game game) : base(game)
        {
        }

        public Bind(Bind effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Bind(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Bound(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}