using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Soaked : Damaged
    {
        public Soaked()
        {
        }

        public Soaked(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }
    }
}