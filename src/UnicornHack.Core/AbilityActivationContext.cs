using System;
using System.Collections.Generic;
using UnicornHack.Effects;

namespace UnicornHack
{
    public class AbilityActivationContext : IDisposable
    {
        public virtual Entity Activator { get; set; }
        public virtual Entity Target { get; set; }
        public virtual Ability Ability { get; set; }
        public virtual AbilityAction AbilityAction { get; set; }
        public ISet<AppliedEffect> AppliedEffects { get; set; } = new HashSet<AppliedEffect>();
        public virtual AbilityActivation AbilityTrigger { get; set; }
        public virtual bool Succeeded { get; set; }
        public virtual bool IsAttack { get; set; }

        public virtual void Add(AppliedEffect effect)
        {
            AppliedEffects.Add(effect.AddReference().Referenced);
        }

        public void Dispose()
        {
            foreach (var appliedEffect in AppliedEffects)
            {
                appliedEffect.RemoveReference();
            }
        }
    }
}