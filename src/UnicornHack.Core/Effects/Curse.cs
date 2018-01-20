using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Curse : DurationEffect
    {
        public Curse()
        {
        }

        public Curse(Game game) : base(game)
        {
        }

        public Curse(Curse effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Curse(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Cursed(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}