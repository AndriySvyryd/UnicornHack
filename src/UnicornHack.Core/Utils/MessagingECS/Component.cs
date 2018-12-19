using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnicornHack.Utils.Caching;

namespace UnicornHack.Utils.MessagingECS
{
    public abstract class Component : IOwnerReferenceable, ITrackable, IPoolable, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public static int NullId = int.MinValue;
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

#if DEBUG
        private readonly List<object> _owners = new List<object>();
#endif
        private int _referenceCount;
        private IObjectPool _pool;
        private bool _tracked;
        private Entity _entity;

        public int ComponentId { get; protected set; }

        public Entity Entity
        {
            get => _entity;
            set
            {
                if (_entity != value)
                {
                    Debug.Assert(_entity == null || value == null);

                    if (value != null)
                    {
                        // Done here because reference count is not persisted
                        ((IOwnerReferenceable)this).AddReference(value);
                        Manager = value.Manager;
                    }

                    _entity = value;
                }
            }
        }

        protected IEntityManager Manager { get; set; }

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
            Debug.Assert(_tracked, $"Component {GetType().Name} is not tracked by {tracker}");

            _tracked = false;
            ((IOwnerReferenceable)this).AddReference(tracker);
            ((IOwnerReferenceable)this).RemoveReference(tracker);
        }

        void IOwnerReferenceable.AddReference(object owner)
        {
#if DEBUG
            _owners.Add(owner);
#endif
            _referenceCount++;
        }

        void IOwnerReferenceable.RemoveReference(object owner)
        {
#if DEBUG
            _owners.Remove(owner);
#endif
            if (--_referenceCount > 0)
            {
                return;
            }

            if (_referenceCount == 0)
            {
                if (_tracked)
                {
                    Manager.RemoveFromSecondaryTracker(this);
                }
                else
                {
                    Clean();
                }
            }
            else
            {
                throw new InvalidOperationException($"Component {ComponentId} is not referenced by object {owner}");
            }
        }

        protected virtual void Clean()
        {
            Debug.Assert(Entity?.HasComponent(ComponentId) != true
                && _referenceCount == 0
                && !_tracked
                && PropertyChanged == null
                && PropertyChanging == null);

            Entity = null;

            _pool?.Return(this);
        }

        void IPoolable.SetPool(IObjectPool pool)
            => _pool = pool;
    }
}
