using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class ItemConsumptionEvent : SensoryEvent
    {
        public virtual Actor Consumer { get; set; }
        public virtual SenseType ConsumerSensed { get; set; }
        public virtual Item Item { get; set; }
        public virtual SenseType ItemSensed { get; set; }

        public static void New(Actor consumer, Item item)
        {
            foreach (var sensor in consumer.Level.Actors)
            {
                var consumerSensed = sensor.CanSense(consumer);
                var objectSensed = sensor.CanSense(item);

                if (consumerSensed == SenseType.None
                    && objectSensed == SenseType.None)
                {
                    continue;
                }

                var @event = new ItemConsumptionEvent
                {
                    Consumer = consumer,
                    ConsumerSensed = consumerSensed,
                    Item = item,
                    ItemSensed = objectSensed
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