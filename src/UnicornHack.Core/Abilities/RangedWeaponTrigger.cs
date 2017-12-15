using System.Linq;
using UnicornHack.Data.Properties;
using UnicornHack.Effects;
using UnicornHack.Events;

namespace UnicornHack.Abilities
{
    public class RangedWeaponTrigger : Trigger
    {
        public RangedWeaponTrigger()
        {
        }

        public RangedWeaponTrigger(Game game) : base(game)
        {
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }

        public override Trigger Copy(Game game) => new RangedWeaponTrigger(game) { Weapon = Weapon.AddReference().Referenced };

        public override void Fire(AbilityActivationContext abilityContext)
        {
            abilityContext.Activator.ActivateAbilities(
                AbilityActivation.OnRangedAttack, abilityContext, useSameContext: false);

            abilityContext.Add(new RangeAttacked(abilityContext) { Weapon = Weapon.AddReference().Referenced });

            var projectile = (Weapon as Launcher)?.Projectile;
            if (projectile == null)
            {
                projectile = Weapon;
                if (abilityContext.Target is Actor actorTarget)
                {
                    Weapon.MoveTo(new LevelCell(
                        actorTarget.Level,
                        actorTarget.LevelX,
                        actorTarget.LevelY));
                }
            }
            else
            {
                Weapon.ActivateAbilities(AbilityActivation.OnRangedAttack, abilityContext, useSameContext: true);
            }

            var hitContext = new AbilityActivationContext
            {
                Activator = abilityContext.Activator,
                Target = abilityContext.Target,
                EffectsToApply = abilityContext.EffectsToApply
            };

            hitContext.Add(new RangeAttacked(abilityContext) { Weapon = projectile.AddReference().Referenced });

            projectile.ActivateAbilities(AbilityActivation.OnRangedAttack, hitContext, useSameContext: true);
        }
    }
}