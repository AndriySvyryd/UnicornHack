using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Shock : DamageEffect
    {
        public Shock()
        {
        }

        public Shock(Game game) : base(game)
        {
        }

        public Shock(Shock effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Shock(this, game);

        // TODO: Causing some mechanical and magical items to trigger
        // TODO: Removes slow
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                null,
                PropertyData.ElectricityResistance.Name);

            abilityContext.Add(new Shocked(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}