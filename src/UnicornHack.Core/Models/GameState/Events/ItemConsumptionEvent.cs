using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState.Events
{
    public class ItemConsumptionEvent : SensoryEvent
    {
        private ItemConsumptionEvent()
        {
        }

        public virtual Actor Consumer { get; set; }
        public virtual SenseType ConsumerSensed { get; set; }
        public virtual Item Object { get; set; }
        public virtual SenseType ObjectSensed { get; set; }

        public static void New(Actor consumer, Item @object)
        {
            foreach (var sensor in consumer.Level.Actors)
            {
                var consumerSensed = sensor.CanSense(consumer);
                var objectSensed = sensor.CanSense(@object);

                if (consumerSensed == SenseType.None
                    && objectSensed == SenseType.None)
                {
                    continue;
                }

                SensoryEvent @event = new ItemConsumptionEvent
                {
                    Consumer = consumer,
                    Object = @object,
                    ConsumerSensed = consumerSensed,
                    ObjectSensed = objectSensed
                };

                sensor.Sense((dynamic)@event);
            }
        }
    }
}