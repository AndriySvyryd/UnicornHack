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

            if (Weapon != null)
            {
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
            }

            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new RangeAttacked(abilityContext) {Weapon = Weapon});
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}