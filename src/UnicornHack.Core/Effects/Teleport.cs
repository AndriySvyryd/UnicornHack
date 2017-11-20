namespace UnicornHack.Effects
{
    public class Teleport : Effect
    {
        public Teleport()
        {
        }

        public Teleport(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Teleport(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Teleported(abilityContext));
        }
    }
}