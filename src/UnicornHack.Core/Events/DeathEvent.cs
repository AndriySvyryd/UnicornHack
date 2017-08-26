namespace UnicornHack.Events
{
    public class DeathEvent : SensoryEvent
    {
        public virtual Actor Deceased { get; set; }
        public virtual int DeceasedId { get; private set; }
        public virtual SenseType DeceasedSensed { get; set; }
        public virtual Item Corpse { get; set; }
        public virtual int? CorpseId { get; private set; }
        public virtual SenseType? CorpseSensed { get; set; }

        public static void New(Actor deceased, Item corpse, int eventOrder)
        {
            foreach (var sensor in deceased.Level.Actors)
            {
                var deceasedSensed = sensor.CanSense(deceased);
                var corpseSensed = corpse == null ? (SenseType?)null : sensor.CanSense(corpse);
                if (deceasedSensed == SenseType.None && (corpseSensed == null || corpseSensed == SenseType.None))
                {
                    continue;
                }

                SensoryEvent @event = new DeathEvent
                {
                    Deceased = deceased,
                    DeceasedSensed = deceasedSensed,
                    Corpse = corpse,
                    CorpseSensed = corpseSensed,
                    EventOrder = eventOrder,
                    Tick = deceased.Level.CurrentTick
                };
                deceased.AddReference();
                corpse?.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Deceased.RemoveReference();
            Corpse?.RemoveReference();
        }
    }
}