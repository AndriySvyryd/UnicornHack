using System.Diagnostics;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class AttackEvent : SensoryEvent
    {
        private AttackEvent()
        {
        }

        public virtual Actor Attacker { get; set; }
        public virtual SenseType AttackerSensed { get; set; }
        public virtual Actor Victim { get; set; }
        public virtual SenseType VictimSensed { get; set; }
        public virtual AttackType AttackType { get; set; }
        public virtual bool Hit { get; set; }
        public virtual Item Weapon { get; set; }
        public virtual Item Projectile { get; set; }
        public virtual int Damage { get; set; }

        public static void New(
            Actor attacker,
            Actor victim,
            AttackType attackType,
            bool hit,
            Item weapon = null,
            Item projectile = null,
            int damage = 0)
        {
            Debug.Assert(attacker.Level == victim.Level);
            Debug.Assert(attackType == AttackType.Weapon || weapon == null);

            foreach (var sensor in attacker.Level.Actors)
            {
                var attackerSensed = sensor.CanSense(attacker);
                var victimSensed = sensor.CanSense(victim);

                if (attackerSensed == SenseType.None
                    && victimSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new AttackEvent
                {
                    Attacker = attacker,
                    AttackerSensed = attackerSensed,
                    Victim = victim,
                    VictimSensed = victimSensed,
                    AttackType = attackType,
                    Hit = hit,
                    Weapon = weapon,
                    Projectile = projectile,
                    Damage = damage
                };

                sensor.Sense((dynamic)@event);
            }
        }
    }
}