using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class RangeAttacked : AppliedEffect
    {
        public RangeAttacked()
        {
        }

        public RangeAttacked(AbilityActivationContext abilityContext, bool targetActivator = false)
            : base(abilityContext, targetActivator)
        {
        }

        public int WeaponId { get; set; }
        public Item Weapon { get; set; }

        protected override void Delete()
        {
            base.Delete();
            Weapon.RemoveReference();
        }
    }
}