namespace UnicornHack.Effects
{
    public class MeleeAttack : Effect
    {
        public MeleeAttack()
        {
        }

        public MeleeAttack(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new MeleeAttack(game) {Weapon = Weapon};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            abilityContext.AbilityTrigger |= AbilityActivation.OnMeleeAttack;

            if (Weapon != null)
            {
                foreach (var weaponAbility in Weapon.Abilities)
                {
                    if (abilityContext.AbilityAction == AbilityAction.Default)
                    {
                        abilityContext.AbilityAction = weaponAbility.Action;
                    }

                    if (abilityContext.Succeeded && weaponAbility.Activation == AbilityActivation.OnMeleeAttack)
                    {
                        weaponAbility.Activate(abilityContext);
                    }
                }
            }

            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new MeleeAttacked(abilityContext) {Weapon = Weapon});
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}