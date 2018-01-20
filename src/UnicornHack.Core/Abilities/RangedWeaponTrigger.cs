using UnicornHack.Effects;

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

        public override Trigger Copy(Game game)
            => new RangedWeaponTrigger(game) {Weapon = Weapon?.AddReference().Referenced};

        public override void Fire(AbilityActivationContext abilityContext)
        {
            abilityContext.Activator.ActivateAbilities(AbilityActivation.OnRangedAttack, abilityContext,
                useSameContext: false);

            abilityContext.Add(new RangeAttacked(abilityContext) {Weapon = Weapon.AddReference().Referenced});

            var targets = Ability.GetTargets(abilityContext);
            var level = abilityContext.Activator.Level;
            var projectile = (Weapon as Launcher)?.Projectile;
            if (projectile == null)
            {
                projectile = Weapon;
                if (abilityContext.TargetEntity is Actor actorTarget)
                {
                    Weapon.MoveTo(new LevelCell(
                        actorTarget.Level,
                        actorTarget.LevelX,
                        actorTarget.LevelY));
                }
                else if (abilityContext.TargetCell != null)
                {
                    Weapon.MoveTo(new LevelCell(
                        level,
                        abilityContext.TargetCell.Value.X,
                        abilityContext.TargetCell.Value.Y));
                }
            }
            else
            {
                Weapon.ActivateAbilities(AbilityActivation.OnRangedAttack, abilityContext, useSameContext: true);
            }

            foreach (var target in targets)
            {
                var hitContext = new AbilityActivationContext
                {
                    Activator = abilityContext.Activator,
                    TargetEntity = target,
                    EffectsToApply = abilityContext.EffectsToApply
                };

                hitContext.Add(new RangeAttacked(hitContext) {Weapon = projectile.AddReference().Referenced});

                projectile.ActivateAbilities(AbilityActivation.OnRangedAttack, hitContext, useSameContext: true);
            }
        }
    }
}