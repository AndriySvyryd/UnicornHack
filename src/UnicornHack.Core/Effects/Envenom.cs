namespace UnicornHack.Effects
{
    public class Envenom : Effect
    {
        public Envenom()
        {
        }

        public Envenom(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Envenom(game) {Damage = Damage};

        // TODO: Decays items
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.AppliedEffects.Add(new Envenomed(abilityContext) {Damage = Damage});
        }
    }
}