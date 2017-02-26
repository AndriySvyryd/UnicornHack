namespace UnicornHack.Events
{
    public class ItemDropEvent : SensoryEvent
    {
        public virtual Actor Dropper { get; set; }
        public virtual int DropperId { get; private set; }
        public virtual SenseType DropperSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual int ItemId { get; private set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor dropper, Item item, int eventOrder)
        {
            foreach (var sensor in dropper.Level.Actors)
            {
                var dropperSensed = sensor.CanSense(dropper);
                var itemSensed = sensor.CanSense(item);

                if (dropperSensed == SenseType.None
                    && itemSensed == SenseType.None)
                {
                    continue;
                }

                SensoryEvent @event = new ItemDropEvent
                {
                    Dropper = dropper,
                    DropperSensed = dropperSensed,
                    Item = item,
                    ItemSensed = itemSensed,
                    EventOrder = eventOrder
                };
                dropper.AddReference();
                item.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Dropper?.RemoveReference();
            Item?.RemoveReference();
        }
    }
}