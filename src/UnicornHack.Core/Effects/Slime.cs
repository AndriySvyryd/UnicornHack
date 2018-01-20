using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Slime : DurationEffect
    {
        public Slime()
        {
        }

        public Slime(Game game) : base(game)
        {
        }

        public Slime(Slime effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Slime(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Slimed(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}