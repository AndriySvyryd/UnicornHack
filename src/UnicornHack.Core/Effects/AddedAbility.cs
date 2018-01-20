using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class AddedAbility : AppliedEffect
    {
        public AddedAbility()
        {
        }

        public AddedAbility(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public Ability Ability { get; set; }

        public override void Remove()
        {
            base.Remove();

            Entity.Remove(Ability);
            Ability.RemoveReference();
        }
    }
}