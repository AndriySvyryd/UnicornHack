using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Effects;

namespace UnicornHack.Events
{
    public class AttackEvent : SensoryEvent
    {
        public Actor Attacker { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int AttackerId { get; private set; }
        public SenseType AttackerSensed { get; set; }
        public Actor Victim { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int VictimId { get; private set; }
        public SenseType VictimSensed { get; set; }
        public AbilityAction AbilityAction { get; set; }
        public ISet<AppliedEffect> AppliedEffects { get; set; } = new HashSet<AppliedEffect>();
        public bool Hit { get; set; }
        public Item Weapon { get; set; }

        public static void New(AbilityActivationContext abilityContext, int eventOrder)
        {
            if (!(abilityContext.Activator is Actor attacker)
                || !(abilityContext.Target is Actor victim))
            {
                return;
            }

            Debug.Assert(attacker.Level == victim.Level);

            var weapon = abilityContext.AppliedEffects.OfType<MeleeAttacked>().Select(m => m.Weapon)
                             .FirstOrDefault(w => w != null)
                         ?? abilityContext.AppliedEffects.OfType<RangeAttacked>().Select(r => r.Weapon)
                             .FirstOrDefault(w => w != null);
            foreach (var sensor in attacker.Level.Actors)
            {
                var attackerSensed = sensor.CanSense(attacker);
                var victimSensed = sensor.CanSense(victim);

                if (attackerSensed == SenseType.None && victimSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new AttackEvent
                {
                    Attacker = attacker,
                    AttackerSensed = attackerSensed,
                    Victim = victim,
                    VictimSensed = victimSensed,
                    AbilityAction = abilityContext.AbilityAction,
                    AppliedEffects = abilityContext.AppliedEffects,
                    Weapon = weapon,
                    Hit = abilityContext.Succeeded,
                    EventOrder = eventOrder,
                    Tick = attacker.Level.CurrentTick
                };
                attacker.AddReference();
                victim.AddReference();
                foreach (var effect in @event.AppliedEffects)
                {
                    effect.AddReference();
                }

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Attacker?.RemoveReference();
            Victim?.RemoveReference();
            foreach (var effect in AppliedEffects)
            {
                effect.RemoveReference();
            }
        }
    }
}