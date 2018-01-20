using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Corrode : DamageEffect
    {
        public Corrode()
        {
        }

        public Corrode(Game game) : base(game)
        {
        }

        public Corrode(Corrode effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Corrode(this, game);

        // TODO: Corrodes items
        // TODO: Removes stoning
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
                PropertyData.AcidResistance.Name);

            abilityContext.Add(new Corroded(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}