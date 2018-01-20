using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Disintegrated : Damaged
    {
        public Disintegrated()
        {
        }

        public Disintegrated(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}