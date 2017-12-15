using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Damaged : AppliedEffect
    {
        public Damaged()
        {
        }

        public Damaged(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public int Damage { get; set; }
    }
}