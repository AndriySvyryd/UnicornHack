using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Soak : DamageEffect
    {
        public Soak()
        {
        }

        public Soak(Game game) : base(game)
        {
        }

        public Soak(Soak effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Soak(this, game);

        // TODO: Rusts, dillutes and blanks items
        // TODO: Removes burning
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
                PropertyData.WaterResistance.Name);

            abilityContext.Add(new Soaked(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}