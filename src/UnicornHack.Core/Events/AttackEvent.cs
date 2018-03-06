using System.Collections.Generic;
using UnicornHack.Abilities;
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

        public static void New(AbilityActivationContext abilityContext, int eventOrder)
        {
            var attacker = abilityContext.Activator as Actor;
            var victim = abilityContext.TargetEntity as Actor;
            var level = attacker?.Level ?? victim?.Level;
            if (level == null)
            {
                return;
            }

            foreach (var sensor in level.Actors)
            {
                var attackerSensed = sensor.CanSense(attacker);
                var victimSensed = sensor.CanSense(victim);

                if (attackerSensed == SenseType.None)
                {
                    if (victimSensed == SenseType.None)
                    {
                        continue;
                    }

                    if ((victimSensed & (SenseType.Sight | SenseType.Touch)) == 0
                        && !abilityContext.Succeeded)
                    {
                        continue;
                    }
                }

                // TODO: One instance per event
                var @event = new AttackEvent
                {
                    Attacker = attacker,
                    AttackerSensed = attackerSensed,
                    Victim = victim,
                    VictimSensed = victimSensed,
                    AbilityAction = abilityContext.AbilityAction,
                    AppliedEffects = abilityContext.AppliedEffects,
                    Hit = abilityContext.Succeeded,
                    EventOrder = eventOrder,
                    Tick = level.CurrentTick
                };
                attacker?.AddReference();
                victim?.AddReference();
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