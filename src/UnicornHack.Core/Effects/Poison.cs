using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Poison : DamageEffect
    {
        public Poison()
        {
        }

        public Poison(Game game) : base(game)
        {
        }

        public Poison(Poison effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Poison(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                null,
                PropertyData.PoisonResistance.Name);

            abilityContext.Add(new Poisoned(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}