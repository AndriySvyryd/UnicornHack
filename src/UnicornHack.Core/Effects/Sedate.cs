using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Sedate : Effect
    {
        public Sedate()
        {
        }

        public Sedate(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Sedate(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Sedated(abilityContext) {Duration = Duration});
        }
    }
}