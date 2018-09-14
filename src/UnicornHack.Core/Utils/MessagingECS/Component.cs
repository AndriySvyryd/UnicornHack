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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetWithNotify<T>(
            T value,
            ref T field,
            [CallerMemberName] string propertyName = "")
        {
            if (NotifyChanging(value, ref field, propertyName, out var oldValue))
            {
                NotifyChanged(propertyName);
                Entity?.HandlePropertyValueChanged(propertyName, oldValue, value, ComponentId, this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool NotifyChanging<T>(
            T value,
            ref T field,
            string propertyName,
            out T oldValue)
        {
            if (Equals(field, value))
            {
                oldValue = default;
                return false;
            }

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            oldValue = field;
            field = value;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

                Entity = null;
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
