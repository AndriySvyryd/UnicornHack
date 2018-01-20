using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Suffocate : DurationEffect
    {
        public Suffocate()
        {
        }

        public Suffocate(Game game) : base(game)
        {
        }

        public Suffocate(Suffocate effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Suffocate(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Suffocated(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}