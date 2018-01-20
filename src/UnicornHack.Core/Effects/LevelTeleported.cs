using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class LevelTeleported : AppliedEffect
    {
        public LevelTeleported()
        {
        }

        public LevelTeleported(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }
    }
}