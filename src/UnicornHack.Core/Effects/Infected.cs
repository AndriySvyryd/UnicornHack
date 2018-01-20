using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Infected : AppliedEffect
    {
        public Infected()
        {
        }

        public Infected(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public int Strength { get; set; }
    }
}