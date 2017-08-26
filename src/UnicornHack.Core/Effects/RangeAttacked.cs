namespace UnicornHack.Effects
{
    public class RangeAttacked : AppliedEffect
    {
        public RangeAttacked()
        {
        }

        public RangeAttacked(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}