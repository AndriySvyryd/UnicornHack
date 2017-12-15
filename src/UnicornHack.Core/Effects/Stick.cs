using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stick : Effect
    {
        public Stick()
        {
        }

        public Stick(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Stick(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stuck(abilityContext) {Duration = Duration});
        }
    }
}