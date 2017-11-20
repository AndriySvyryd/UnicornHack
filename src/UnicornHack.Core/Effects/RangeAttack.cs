using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class RangeAttack : Effect
    {
        public RangeAttack()
        {
        }

        public RangeAttack(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new RangeAttack(game) {Weapon = Weapon};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            abilityContext.AbilityTrigger |= AbilityActivation.OnRangedAttack;

            foreach (var weaponAbility in Weapon.Abilities)
            {
                if (abilityContext.AbilityAction == AbilityAction.Default)
                {
                    abilityContext.AbilityAction = weaponAbility.Action;
                }

                if (abilityContext.Succeeded && weaponAbility.Activation == AbilityActivation.OnRangedAttack)
                {
                    weaponAbility.Activate(abilityContext);
                }
            }

            if (abilityContext.AbilityAction == AbilityAction.Default)
            {
                abilityContext.AbilityAction = AbilityAction.Hit;
            }

            abilityContext.Add(
                new RangeAttacked(abilityContext) {Weapon = Weapon.AddReference().Referenced});

            if (!Weapon.GetProperty<bool>(PropertyData.InfiniteAmmo.Name)
                && abilityContext.Target is Actor actorTarget)
            {
                Weapon.MoveTo(new LevelCell(
                    actorTarget.Level,
                    actorTarget.LevelX,
                    actorTarget.LevelY));
            }
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}