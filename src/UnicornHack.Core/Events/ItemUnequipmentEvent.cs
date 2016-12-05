namespace UnicornHack.Events
{
    public class ItemUnequipmentEvent : SensoryEvent
    {
        public virtual Actor Unequipper { get; set; }
        public virtual SenseType UnequipperSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor unequipper, Item item)
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
                    ItemSensed = itemSensed
                };
                item.AddReference();

                sensor.Sense(@event);
            }
        }

        public override void Delete()
        {
            base.Delete();
            Item?.RemoveReference();
        }
    }
}