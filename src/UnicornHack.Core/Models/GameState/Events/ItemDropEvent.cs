using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class ItemDropEvent : SensoryEvent
    {
        protected ItemDropEvent()
        {
        }

        public virtual Actor Dropper { get; set; }
        public virtual SenseType DropperSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor dropper, Item item)
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