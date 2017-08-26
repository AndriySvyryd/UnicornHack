namespace UnicornHack.Effects
{
    public class Soak : Effect
    {
        public Soak()
        {
        }

        public Soak(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Soak(game) {Damage = Damage};

        // TODO: Only does damage to actors with water weakness, rusts, dillutes and blanks items
        // TODO: Removes burning
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Target.ChangeCurrentHP(-1 * Damage);
            abilityContext.AppliedEffects.Add(new Soaked(abilityContext) {Damage = Damage});
        }
    }
}