using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Burn : DamageEffect
    {
        public Burn()
        {
        }

        public Burn(Game game) : base(game)
        {
        }

        public Burn(Burn effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Burn(this, game);

        // TODO: Burns items
        // TODO: Removes slime, wet, frozen
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
                PropertyData.FireResistance.Name);

            abilityContext.Add(new Burned(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}