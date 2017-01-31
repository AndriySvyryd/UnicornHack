using System.Diagnostics;

namespace UnicornHack.Events
{
    public class AttackEvent : SensoryEvent
    {
        public virtual Actor Attacker { get; set; }
        public virtual int AttackerId { get; private set; }
        public virtual SenseType AttackerSensed { get; set; }
        public virtual Actor Victim { get; set; }
        public virtual int VictimId { get; private set; }
        public virtual SenseType VictimSensed { get; set; }
        public virtual Ability Ability { get; set; }
        public virtual int AbilityId { get; private set; }
        public virtual bool Hit { get; set; }

        public static void New(AbilityActivationContext abilityContext, int turnOrder)
        {
            var attacker = abilityContext.Activator;
            var victim = abilityContext.Target;
            Debug.Assert(attacker.Level == victim.Level);

            foreach (var sensor in attacker.Level.Actors)
            {
                var attackerSensed = sensor.CanSense(attacker);
                var victimSensed = sensor.CanSense(victim);

                if ((attackerSensed == SenseType.None)
                    && (victimSensed == SenseType.None))
                {
                    continue;
                }

                var @event = new AttackEvent
                {
                    Attacker = attacker,
                    AttackerSensed = attackerSensed,
                    Victim = victim,
                    VictimSensed = victimSensed,
                    Ability = abilityContext.Ability,
                    Hit = abilityContext.Succeeded,
                    TurnOrder = turnOrder
                };
                attacker.AddReference();
                victim.AddReference();
                abilityContext.Ability.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Attacker?.RemoveReference();
            Victim?.RemoveReference();
            Ability?.RemoveReference();
        }
    }
}