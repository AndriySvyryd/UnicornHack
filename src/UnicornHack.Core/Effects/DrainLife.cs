namespace UnicornHack.Effects
{
    public class DrainLife : Effect
    {
        public DrainLife()
        {
        }

        public DrainLife(Game game) : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new DrainLife(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Amount);
            (abilityContext.Activator as Actor)?.ChangeCurrentHP(Amount);
            abilityContext.Add(new LifeDrained(abilityContext) {Amount = Amount});
        }
    }
}