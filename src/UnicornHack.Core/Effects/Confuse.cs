using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Confuse : DurationEffect
    {
        public Confuse()
        {
        }

        public Confuse(Game game) : base(game)
        {
        }

        public Confuse(Confuse effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Confuse(this, game);

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