using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class WaterDamage : DamageEffect
    {
        public WaterDamage()
        {
        }

        public WaterDamage(Game game) : base(game)
        {
        }

        // Only does damage to actors with water weakness, rusts, dillutes and blanks items
        // Removes burning
        public override void Apply(AbilityActivationContext abilityContext)
        {
            base.Apply(abilityContext);
            if (abilityContext.Succeeded)
            {
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public override Effect Instantiate(Game game) => new WaterDamage(game) {Damage = Damage};
    }
}