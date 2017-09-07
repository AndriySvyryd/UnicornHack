namespace UnicornHack.Effects
{
    public class Burn : Effect
    {
        public Burn()
        {
        }

        public Burn(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Burn(game) {Damage = Damage};

        // TODO: Burns items
        // TODO: Removes slime, wet, frozen
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.AppliedEffects.Add(new Burned(abilityContext) {Damage = Damage});
        }
    }
}