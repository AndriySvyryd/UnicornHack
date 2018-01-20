using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class ItemStolen : AppliedEffect
    {
        public ItemStolen()
        {
        }

        public ItemStolen(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}