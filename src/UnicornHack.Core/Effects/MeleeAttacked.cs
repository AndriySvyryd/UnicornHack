namespace UnicornHack.Effects
{
    public class MeleeAttacked : AppliedEffect
    {
        public MeleeAttacked()
        {
        }

        public MeleeAttacked(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }

        protected override void Delete()
        {
            base.Delete();
            Weapon?.RemoveReference();
        }
    }
}