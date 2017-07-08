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
                Weapon = Weapon?.AddReference().Referenced
            };

        protected override void Delete()
        {
            base.Delete();
            Weapon?.RemoveReference();
        }

        public override void Apply(AbilityActivationContext abilityContext)
        {
            abilityContext.AbilityTrigger |= AbilityActivation.OnMeleeAttack;

            if (Weapon != null)
            {
                foreach (var weaponAbility in Weapon.Abilities)
                {
                    if (abilityContext.AbilityResult.Action == AbilityAction.Default)
                    {
                        abilityContext.AbilityResult.Action = weaponAbility.Action;
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
                abilityContext.AbilityResult.Effects.Add(Instantiate(Game));
            }
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}