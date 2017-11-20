namespace UnicornHack.Effects
{
    public class Corrode : Effect
    {
        public Corrode()
        {
        }

        public Corrode(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Corrode(game) {Damage = Damage};

        // TODO: Corrodes items
        // TODO: Removes stoning
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.Add(new Corroded(abilityContext) {Damage = Damage});
        }
    }
}