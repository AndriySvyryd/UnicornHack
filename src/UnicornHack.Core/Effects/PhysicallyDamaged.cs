using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class PhysicallyDamaged : Damaged
    {
        public PhysicallyDamaged()
        {
        }

        public PhysicallyDamaged(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}