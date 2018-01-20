using UnicornHack.Effects;

namespace UnicornHack.Abilities
{
    public class MeleeWeaponTrigger : Trigger
    {
        public MeleeWeaponTrigger()
        {
        }

        public MeleeWeaponTrigger(Game game) : base(game)
        {
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }

        public override Trigger Copy(Game game)
            => new MeleeWeaponTrigger(game) {Weapon = Weapon?.AddReference().Referenced};

        public override void Fire(AbilityActivationContext abilityContext)
        {
            abilityContext.Activator.ActivateAbilities(AbilityActivation.OnMeleeAttack, abilityContext,
                useSameContext: false);

            var targetsFound = false;
            foreach (var target in Ability.GetTargets(abilityContext))
            {
                targetsFound = true;
                var hitContext = new AbilityActivationContext
                {
                    Activator = abilityContext.Activator,
                    TargetEntity = target,
                    EffectsToApply = abilityContext.EffectsToApply
                };

                hitContext.Add(new MeleeAttacked(abilityContext) {Weapon = Weapon.AddReference().Referenced});
                Weapon.ActivateAbilities(AbilityActivation.OnMeleeAttack, hitContext, useSameContext: true);
            }

            if (!targetsFound)
            {
                abilityContext.Add(new MeleeAttacked(abilityContext) {Weapon = Weapon.AddReference().Referenced});
                Weapon.ActivateAbilities(AbilityActivation.OnMeleeAttack, abilityContext, useSameContext: true);
            }
        }
    }
}