using System.Collections.Generic;
using UnicornHack.Effects;

namespace UnicornHack
{
    public class AbilityActivationContext
    {
        public virtual Entity Activator { get; set; }
        public virtual Entity Target { get; set; }
        public virtual Ability Ability { get; set; }
        public virtual AbilityAction AbilityAction { get; set; }
        public ISet<AppliedEffect> AppliedEffects { get; set; } = new HashSet<AppliedEffect>();
        public virtual AbilityActivation AbilityTrigger { get; set; }
        public virtual bool Succeeded { get; set; }
        public virtual bool IsAttack { get; set; }
    }
}