namespace UnicornHack.Events
{
    public class ItemUnequipmentEvent : SensoryEvent
    {
        public virtual Actor Unequipper { get; set; }
        public virtual int UnequipperId { get; private set; }
        public virtual SenseType UnequipperSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual int ItemId { get; private set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor unequipper, Item item, int eventOrder)
        {
            foreach (var sensor in unequipper.Level.Actors)
            {
                var unequipperSensed = sensor.CanSense(unequipper);
                var itemSensed = sensor.CanSense(item);

                if (unequipperSensed == SenseType.None
                    && itemSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new ItemUnequipmentEvent
                {
                    Unequipper = unequipper,
                    UnequipperSensed = unequipperSensed,
                    Item = item,
                    ItemSensed = itemSensed,
                    EventOrder = eventOrder
                };
                unequipper.AddReference();
                item.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Unequipper?.RemoveReference();
            Item?.RemoveReference();
        }
    }
}