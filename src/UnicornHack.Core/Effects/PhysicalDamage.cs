using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class PhysicalDamage : Effect
    {
        public PhysicalDamage()
        {
        }

        public PhysicalDamage(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new PhysicalDamage(game) {Damage = Damage};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if (Damage != 0)
            {
                (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Damage);
                abilityContext.Add(new PhysicallyDamaged(abilityContext) {Damage = Damage});
            }
        }
    }
}