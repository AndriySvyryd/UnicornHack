using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class MeleeAttack : Effect
    {
        public MeleeAttack()
        {
        }

        public MeleeAttack(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new MeleeAttack(game)
            {
                WeaponId = WeaponId,
                Weapon = Weapon
            };

        public override void Apply(AbilityActivationContext abilityContext)
        {
            abilityContext.AbilityTrigger |= AbilityActivation.OnMeleeAttack;

            if (Weapon != null)
            {
                foreach (var weaponAbility in Weapon.Abilities)
                {
                    if (abilityContext.Ability.Action == AbilityAction.Default)
                    {
                        abilityContext.Ability.Action = weaponAbility.Action;
                    }

                    if (abilityContext.Succeeded
                        && weaponAbility.Activation == AbilityActivation.OnMeleeAttack)
                    {
                        weaponAbility.Activate(abilityContext);
                    }
                }
            }

            if (abilityContext.Succeeded)
            {
                abilityContext.Ability.Effects.Add(Instantiate(Game));
            }
        }


        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}