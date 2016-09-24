using System.Diagnostics;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class AttackEvent : SensoryEvent
    {
        public virtual Actor Attacker { get; set; }
        public virtual SenseType AttackerSensed { get; set; }
        public virtual Actor Victim { get; set; }
        public virtual SenseType VictimSensed { get; set; }
        public virtual AbilityAction AbilityAction { get; set; }
        public virtual bool Hit { get; set; }
        public virtual Item Weapon { get; set; }
        public virtual Item Projectile { get; set; }
        public virtual int Damage { get; set; }

        public static void New(
            Actor attacker,
            Actor victim,
            AbilityAction attackType,
            bool hit,
            Item weapon = null,
            Item projectile = null,
            int damage = 0)
        {
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
                    AbilityAction = attackType,
                    Hit = hit,
                    Weapon = weapon,
                    Projectile = projectile,
                    Damage = damage
                };

                sensor.Sense(@event);
            }
        }
    }
}