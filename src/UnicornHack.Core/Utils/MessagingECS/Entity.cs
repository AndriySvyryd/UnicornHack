using System;
using System.Diagnostics;
using UnicornHack.Utils.Caching;
using System.Collections.Generic;
using System.Text;

namespace UnicornHack.Utils.MessagingECS
{
    // TODO: Perf: Separate into BaseEntity and ArrayEntity and allow storing components directly into properties
    public class Entity : NotificationEntity, IOwnerReferenceable, ITrackable, IPoolable
    {
        private Component[] _components;

#if DEBUG
        private readonly List<object> _owners = new List<object>();
#endif
        private IObjectPool _pool;
        private int _referenceCount;
        private bool _tracked;
        private int _id;

        public int Id
        {
            get => _id;
            private set => SetWithNotify(value, ref _id);
        }

        public IEntityManager Manager { get; private set; }

        public virtual void Initialize(int id, int componentCount, IEntityManager manager)
        {
            Debug.Assert(Manager == null);

            Id = id;
            Manager = manager;
            if (_components == null
                || _components.Length != componentCount)
            {
                _components = new Component[componentCount];
            }
        }

        public TComponent AddComponent<TComponent>(int componentId)
            where TComponent : Component, new()
        {
#if DEBUG
            if (_components[componentId] != null)
            {
                throw new InvalidOperationException(
                    $"Entity {Id} already contains component of type {typeof(TComponent).Name}");
            }
#endif

            return AddComponent(Manager.CreateComponent<TComponent>(componentId));
        }

        public TComponent AddComponent<TComponent>(TComponent component)
            where TComponent : Component, new()
        {
            var componentId = component.ComponentId;
            var propertyName = GetComponentPropertyName(componentId);

            if (propertyName != null)
            {
                FirePropertyChanging(propertyName);
            }

            _components[componentId] = component;

            if (component is IKeepAliveComponent)
            {
                AddReference(component);
            }

            InitializeComponent(component);

            if (propertyName != null)
            {
                FirePropertyChanged(propertyName);
            }

            Manager?.HandleComponentAdded(component);
            return component;
        }

        protected virtual void InitializeComponent(Component component)
        {
            Debug.Assert(component.Entity == null || component.Entity == this,
                $"Component {component.GetType().Name} is already owned by entity {component.Entity?.Id}");

            component.Entity = this;
        }

        public Component FindComponent(int componentId)
            => _components[componentId];

        public TComponent GetOrAddComponent<TComponent>(int componentId)
            where TComponent : Component, new()
            => (TComponent)_components[componentId] ?? AddComponent<TComponent>(componentId);

        public Component RemoveComponent(int componentId)
        {
            var component = _components[componentId];
            if (component == null)
            {
                return null;
            }

            RemoveComponent(component);
            return component;
        }

        public void RemoveComponent(Component component)
        {
            if (component.Entity == null)
            {
                return;
            }

            var componentId = component.ComponentId;
            if (component.Entity != this)
            {
                throw new InvalidOperationException(
                    $"The component '{componentId}' cannot be removed from entity '{Id}' as it belongs to the entity '{component.Entity.Id}'");
            }

            var propertyName = GetComponentPropertyName(componentId);
            if (propertyName != null)
            {
                FirePropertyChanging(propertyName);
            }

            _components[componentId] = null;

            Manager?.HandleComponentRemoved(component);

            ((IOwnerReferenceable)component).RemoveReference(this);

            if (component is IKeepAliveComponent)
            {
                RemoveReference(component);
            }

            if (propertyName != null)
            {
                FirePropertyChanged(propertyName);
            }
        }

        public bool HasComponent(int componentId)
            => _components[componentId] != null;

        public bool HasComponents(int[] ids)
        {
            foreach (var id in ids)
            {
                if (_components[id] == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasAnyComponent(int[] ids)
        {
            foreach (var id in ids)
            {
                if (_components[id] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void ForEachComponent<TState>(TState state, Action<TState, int, Component> componentAction)
        {
            for (var i = 0; i < _components.Length; i++)
            {
                var component = _components[i];
                if (component != null)
                {
                    componentAction(state, i, component);
                }
            }
        }

        public void HandlePropertyValueChanged<T>(
            string propertyName, T oldValue, T newValue, int componentId, Component component)
            => Manager.HandlePropertyValueChanged(propertyName, oldValue, newValue, componentId, component);

        public void HandlePropertyValuesChanged(IReadOnlyList<IPropertyValueChange> changes)
            => Manager.HandlePropertyValuesChanged(changes);

        protected virtual string GetComponentPropertyName(int componentId) => null;

        public OwnerTransientReference<Entity, TOwner> AddReference<TOwner>(TOwner owner)
            => new OwnerTransientReference<Entity, TOwner>(this, owner);

        void IOwnerReferenceable.AddReference(object owner)
        {
#if DEBUG
            _owners.Add(owner);
#endif
            _referenceCount++;
        }

        public void RemoveReference(object owner)
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
                ForEachComponent(this, (e, _, c) => e.RemoveComponent(c));
                if (_tracked)
                {
                    Manager.RemoveFromSecondaryTracker(this);
                }
                else
                {
                    Manager.RemoveEntity(this);

                    _id = 0;
                    Manager = null;
                    _pool?.Return(this);
                }
            }
            else
            {
                throw new InvalidOperationException($"Entity {Id} is not referenced by object {owner}");
            }
        }

        void IPoolable.SetPool(IObjectPool pool) => _pool = pool;

        void ITrackable.StartTracking(object tracker)
        {
            Debug.Assert(!_tracked, $"Entity {Id} is already tracked by {tracker}");

            _tracked = true;
        }

        void ITrackable.StopTracking(object tracker)
        {
            Debug.Assert(_tracked, $"Entity {Id} is not tracked by {tracker}");

            _tracked = false;
            ((IOwnerReferenceable)this).AddReference(tracker);
            RemoveReference(tracker);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder("Entity: ");
            stringBuilder.Append(Id);
            ForEachComponent(stringBuilder, (sb, componentId, _) =>
            {
                sb.Append(" ");
                sb.Append(GetComponentPropertyName(componentId) ?? componentId.ToString());
            });

            return stringBuilder.ToString();
        }
    }
}
