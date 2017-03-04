using UnicornHack.Utils;

namespace UnicornHack.Events
{
    public abstract class SensoryEvent : IReferenceable
    {
        public virtual int Id { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int SensorId { get; private set; }
        public virtual Player Sensor { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual int EventOrder { get; set; }
        public virtual int Tick { get; set; }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<SensoryEvent> AddReference()
        {
            return new TransientReference<SensoryEvent>(this);
        }

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                Delete();
            }
        }

        protected virtual void Delete()
        {
            Game.Delete(this);
        }
    }
}