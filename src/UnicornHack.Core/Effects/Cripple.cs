using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Cripple : DurationEffect
    {
        public Cripple()
        {
        }

        public Cripple(Game game) : base(game)
        {
        }

        public Cripple(Cripple effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Cripple(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Confused(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}