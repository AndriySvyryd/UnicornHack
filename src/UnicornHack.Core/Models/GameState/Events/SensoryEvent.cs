namespace UnicornHack.Models.GameState.Events
{
    public class SensoryEvent
    {
        public virtual int Id { get; set; }
        public virtual int SensorId { get; set; }
        public virtual Actor Sensor { get; set; }
    }
}