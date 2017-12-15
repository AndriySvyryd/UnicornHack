using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Infected : AppliedEffect
    {
        public Infected()
        {
        }

        public Infected(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Strength { get; set; }
    }
}