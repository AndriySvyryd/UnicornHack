using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack.Abilities
{
    public class AbilityActivationContext : IDisposable
    {
        public virtual Entity Activator { get; set; }
        public virtual Entity TargetEntity { get; set; }
        public virtual Point? TargetCell { get; set; }
        public virtual Ability Ability { get; set; }
        public virtual AbilityAction AbilityAction { get; set; }
        public ISet<AppliedEffect> AppliedEffects { get; set; } = new HashSet<AppliedEffect>();
        public ImmutableList<Effect> EffectsToApply { get; set; }
        public virtual bool Succeeded { get; set; } = true;

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