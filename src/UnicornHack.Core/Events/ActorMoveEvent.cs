namespace UnicornHack.Events
{
    public class ActorMoveEvent : SensoryEvent
    {
        protected ActorMoveEvent()
        {
        }

        public virtual Actor Mover { get; set; }
        public virtual int MoverId { get; private set; }
        public virtual SenseType MoverSensed { get; set; }
        public virtual Actor Movee { get; set; }
        public virtual int? MoveeId { get; private set; }
        public virtual SenseType? MoveeSensed { get; set; }

        public static void New(Actor mover, Actor movee, int eventOrder)
        {
            foreach (var sensor in mover.Level.Actors)
            {
                var moverSensed = sensor.CanSense(mover);
                var moveeSensed = movee == null ? (SenseType?)null : sensor.CanSense(movee);

                if (moverSensed == SenseType.None && (moveeSensed == null || moveeSensed.Value == SenseType.None))
                {
                    continue;
                }

                SensoryEvent @event = new ActorMoveEvent
                {
                    Mover = mover,
                    MoverSensed = moverSensed,
                    EventOrder = eventOrder,
                    Tick = mover.Level.CurrentTick
                };
                mover.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Mover?.RemoveReference();
            Movee?.RemoveReference();
        }
    }
}