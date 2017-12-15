using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Poisoned : Damaged
    {
        public Poisoned()
        {
        }

        public Poisoned(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }
    }
}