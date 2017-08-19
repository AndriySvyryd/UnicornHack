namespace UnicornHack.Events
{
    public class ItemEquipmentEvent : SensoryEvent
    {
        public virtual Actor Equipper { get; set; }
        public virtual int EquipperId { get; private set; }
        public virtual SenseType EquipperSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual int ItemId { get; private set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor equipper, Item item, int eventOrder)
        {
            foreach (var sensor in equipper.Level.Actors)
            {
                var equipperSensed = sensor.CanSense(equipper);
                var itemSensed = sensor.CanSense(item);

                if (equipperSensed == SenseType.None && itemSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new ItemEquipmentEvent
                {
                    Equipper = equipper,
                    EquipperSensed = equipperSensed,
                    Item = item,
                    ItemSensed = itemSensed,
                    EventOrder = eventOrder,
                    Tick = equipper.Level.CurrentTick
                };
                equipper.AddReference();
                item.AddReference();

                sensor.Sense(@event);
            }
        }

        protected override void Delete()
        {
            base.Delete();
            Equipper?.RemoveReference();
            Item?.RemoveReference();
        }
    }
}