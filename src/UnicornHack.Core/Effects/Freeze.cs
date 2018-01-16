using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Freeze : Effect
    {
        public Freeze()
        {
        }

        public Freeze(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Freeze(game) {Damage = Damage};

        // TODO: Freezes items
        // TODO: Slows, removes burning, dissolving
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.TargetEntity as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.Add(new Frozen(abilityContext) {Damage = Damage});
        }
    }
}