namespace UnicornHack.Effects
{
    public class Confuse : Effect
    {
        public Confuse()
        {
        }

        public Confuse(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Confuse(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Confused(abilityContext) {Duration = Duration});
        }
    }
}