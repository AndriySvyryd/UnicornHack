namespace UnicornHack.Events
{
    public class ItemPickUpEvent : SensoryEvent
    {
        public virtual Actor Picker { get; set; }
        public virtual int PickerId { get; private set; }
        public virtual SenseType PickerSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual int ItemId { get; private set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor picker, Item item, int eventOrder)
        {
            foreach (var sensor in picker.Level.Actors)
            {
                var pickerSensed = sensor.CanSense(picker);
                var itemSensed = sensor.CanSense(item);

                if (pickerSensed == SenseType.None
                    && itemSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new ItemPickUpEvent
                {
                    Picker = picker,
                    PickerSensed = pickerSensed,
                    Item = item,
                    ItemSensed = itemSensed,
                    EventOrder = eventOrder
                };
                picker.AddReference();
                item.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Picker?.RemoveReference();
            Item?.RemoveReference();
        }
    }
}