namespace UnicornHack.Models.GameState.Events
{
    public abstract class SensoryEvent
    {
        public virtual int Id { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int SensorId { get; private set; }
        public virtual Actor Sensor { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game => Sensor.Game;

        public virtual void Delete()
        {
            Game.Delete(this);
        }
    }
}