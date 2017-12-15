using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class MagicallyDamaged : Damaged
    {
        public MagicallyDamaged()
        {
        }

        public MagicallyDamaged(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }
    }
}