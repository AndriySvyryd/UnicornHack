using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnicornHack.Utils.Caching;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class Component : ITrackable, IPoolable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public static int NullId = int.MinValue;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        private IObjectPool _pool;
        private bool _tracked;

        public int ComponentId { get; protected set; }
        public Entity Entity { get; set; }

        protected void SetWithNotify<T>(
            T value,
            ref T field,
            [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
            {
                return;
            }

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            var oldValue = field;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Entity?.HandlePropertyValueChanged(propertyName, oldValue, value, ComponentId, this);
        }

        void ITrackable.StartTracking(object tracker)
        {
            Debug.Assert(!_tracked, $"Component {GetType().Name} is already tracked by {tracker}");
            _tracked = true;
        }

        void ITrackable.StopTracking(object tracker)
        {
            _tracked = false;
            if (Entity == null)
            {
                Clean();
                return;
            }

            Debug.Assert(!Entity.HasComponent(ComponentId));
        }

        public void HandleRemovedFromEntity(IEntityManager manager)
        {
            if (!_tracked)
            {
                Debug.Assert(PropertyChanged == null);
                Debug.Assert(PropertyChanging == null);

                Clean();
            }
            else
            {
                Entity = null;
                manager?.RemoveFromSecondaryTracker(this);
            }
        }

        void IPoolable.SetPool(IObjectPool pool)
            => _pool = pool;

        protected virtual void Clean()
            => _pool?.Return(this);
    }
}
