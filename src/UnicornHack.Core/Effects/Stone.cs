namespace UnicornHack.Effects
{
    public class Stone : Effect
    {
        public Stone()
        {
        }

        public Stone(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Stone(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stoned(abilityContext) {Duration = Duration});
        }
    }
}