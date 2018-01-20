using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public abstract class DamageEffect : Effect
    {
        protected DamageEffect()
        {
        }

        protected DamageEffect(Game game) : base(game)
        {
        }

        protected DamageEffect(DamageEffect effect, Game game)
            : base(effect, game)
            => Damage = effect.Damage;

        public int Damage { get; set; }

        protected int ApplyDamage(AbilityActivationContext abilityContext, string absorption, string resistance)
        {
            var damage = Damage;
            if (!((TargetActivator ? abilityContext.Activator : abilityContext.TargetEntity) is Actor targetActor))
            {
                return damage;
            }

            damage = Damage;
            if (absorption != null)
            {
                damage -= targetActor.GetProperty<int>(absorption);
            }

            if (damage < 0)
            {
                damage = 0;
            }

            damage = (damage * (100 - targetActor.GetProperty<int>(resistance))) / 100;
            targetActor.ChangeCurrentHP(-damage);

            return damage;
        }
    }
}