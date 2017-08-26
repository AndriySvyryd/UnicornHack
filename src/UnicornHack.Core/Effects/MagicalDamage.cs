namespace UnicornHack.Effects
{
    public class MagicalDamage : Effect
    {
        public MagicalDamage()
        {
        }

        public MagicalDamage(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new MagicalDamage(game) {Damage = Damage};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Target.ChangeCurrentHP(-1 * Damage);
            abilityContext.AppliedEffects.Add(new MagicallyDamaged(abilityContext) {Damage = Damage});
        }
    }
}