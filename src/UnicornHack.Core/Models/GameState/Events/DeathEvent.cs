using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class DeathEvent : SensoryEvent
    {
        protected DeathEvent()
        {
        }

        public virtual Actor Deceased { get; set; }
        public virtual SenseType DeceasedSensed { get; set; }
        public virtual Item Corpse { get; set; }
        public virtual SenseType? CorpseSensed { get; set; }

        public static void New(Actor deceased, Item corpse)
        {
            foreach (var sensor in deceased.Level.Actors)
            {
                var deceasedSensed = sensor.CanSense(deceased);
                var corpseSensed = corpse == null
                    ? (SenseType?)null
                    : sensor.CanSense(corpse);
                if ((deceasedSensed == SenseType.None)
                    && ((corpseSensed == null)
                        || (corpseSensed == SenseType.None)))
                {
                    continue;
                }

                SensoryEvent @event = new DeathEvent
                {
                    Deceased = deceased,
                    DeceasedSensed = deceasedSensed,
                    Corpse = corpse,
                    CorpseSensed = corpseSensed
                };

                sensor.Sense(@event);
            }
        }
    }
}