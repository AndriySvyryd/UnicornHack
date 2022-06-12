using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class LookupEntityRelationship<TEntity, TKey, TDictionary> : EntityRelationshipBase<TEntity>
        where TEntity : Entity, new()
        where TDictionary : class, IDictionary<TKey, TEntity>, new()
    {
        private DependentsGroup _dependents;
        private readonly IKeyValueGetter<TEntity, TKey> _secondaryKeyValueGetter;

        private static readonly IReadOnlyDictionary<TKey, TEntity> EmptyDictionary = new Dictionary<TKey, TEntity>();

        public LookupEntityRelationship(
            string name,
            IEntityGroup<TEntity> dependentGroup,
            IEntityGroup<TEntity> principalGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
            Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
            Expression<Func<TEntity, TDictionary>> getDependent = null,
            Expression<Func<TEntity, TEntity>> getPrincipal = null,
            Func<TDictionary> factory = null,
            bool keepPrincipalAlive = false,
            bool keepDependentAlive = false)
            : this(
                name,
                dependentGroup,
                principalGroup,
                keyValueGetter,
                secondaryKeyValueGetter,
                handlePrincipalDeleted,
                keepPrincipalAlive,
                keepDependentAlive)
        {
            Accessor = getDependent == null
                ? new ExternalCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>()
                : new PropertyCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>(
                    getDependent, getPrincipal);
            Accessor.SetDefaultFactory(factory ?? (() => new TDictionary()));
        }

        public LookupEntityRelationship(
            string name,
            IEntityGroup<TEntity> dependentGroup,
            IEntityGroup<TEntity> principalGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
            Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
            Func<TEntity, TDictionary> getDependent,
            Action<TEntity, TDictionary> setDependent,
            Func<Component, TDictionary> componentGetDependent,
            Func<TEntity, TEntity> getPrincipal = null,
            Action<TEntity, TEntity> setPrincipal = null,
            Func<Component, TEntity> componentGetPrincipal = null,
            Func<TDictionary> factory = null,
            bool keepPrincipalAlive = false,
            bool keepDependentAlive = false)
            : this(
                name,
                dependentGroup,
                principalGroup,
                keyValueGetter,
                secondaryKeyValueGetter,
                handlePrincipalDeleted,
                keepPrincipalAlive,
                keepDependentAlive)
        {
            Accessor = new PropertyCollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>>(
                    getDependent, setDependent, componentGetDependent, getPrincipal, setPrincipal, componentGetPrincipal);
            Accessor.SetDefaultFactory(factory ?? (() => new TDictionary()));
        }

        private LookupEntityRelationship(
            string name,
            IEntityGroup<TEntity> dependentGroup,
            IEntityGroup<TEntity> principalGroup,
            IKeyValueGetter<TEntity, int> keyValueGetter,
            IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
            Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
            bool keepPrincipalAlive = false,
            bool keepDependentAlive = false)
            : base(
                name,
                dependentGroup,
                principalGroup,
                keyValueGetter,
                handlePrincipalDeleted,
                keepPrincipalAlive,
                keepDependentAlive)
        {
            principalGroup.AddListener(new PrincipalGroupListener(this));
            _secondaryKeyValueGetter = secondaryKeyValueGetter;
        }

        private ICollectionAccessor<TEntity, TDictionary, KeyValuePair<TKey, TEntity>> Accessor { get; }
        private Dictionary<int, HashSet<TEntity>> OrphanedEntities { get; } = new();

        public IEntityGroup<TEntity> Dependents
            => _dependents ??= new DependentsGroup(this);

        public IReadOnlyDictionary<TKey, TEntity> GetDependents(TEntity principal, Component removedComponent = null)
            => (IReadOnlyDictionary<TKey, TEntity>)Accessor.GetDependents(principal, removedComponent) ?? EmptyDictionary;

        public TEntity GetPrincipal(TEntity dependent, Component removedComponent = null)
            => Accessor.TryGetPrincipal(dependent, out var principal, removedComponent)
                ? principal
                : KeyValueGetter.TryGetKey(new EntityChange<TEntity>(dependent), ValueType.Current, out var key)
                    ? FindPrincipal(key, dependent, fallback: false)
                    : null;

        protected override bool TryAddEntity(int key, in EntityChange<TEntity> entityChange)
        {
            if (!_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var secondaryKey))
            {
                return false;
            }

            var principal = FindPrincipal(key, entityChange.Entity, fallback: false);
            if (principal != null)
            {
                Accessor.GetOrCreateDependents(principal).Add(secondaryKey, entityChange.Entity);
                OnEntityAdded(entityChange, principal);
                return true;
            }

            if (!OrphanedEntities.TryGetValue(key, out var orphanedEntities))
            {
                orphanedEntities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                OrphanedEntities.Add(key, orphanedEntities);
            }
            orphanedEntities.Add(entityChange.Entity);

            return false;
        }

        private void OnEntityAdded(in EntityChange<TEntity> entityChange, TEntity principal)
        {
            Accessor.SetPrincipal(entityChange.Entity, principal);

            _dependents?.Add(entityChange, principal);

            ((IEntityRelationshipBase<TEntity>)this).OnEntityAdded(entityChange.Entity, principal);
        }

        protected override bool TryRemoveEntity(int key, in EntityChange<TEntity> entityChange)
        {
            if (!_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var secondaryKey))
            {
                return false;
            }

            var principal = FindPrincipal(key, entityChange.Entity, fallback: true);
            if (principal == null)
            {
                return false;
            }

            var dependentEntities = Accessor.GetDependents(principal);
            if (dependentEntities != null
                && dependentEntities.Remove(secondaryKey))
            {
                OnEntityRemoved(entityChange, principal);
                return true;
            }

            Debug.Assert(OrphanedEntities.Count == 0, $"Orphaned entities found in {Name} for principal {key}");

            return false;
        }

        private void OnEntityRemoved(in EntityChange<TEntity> entityChange, TEntity principal)
        {
            Accessor.SetPrincipal(entityChange.Entity, null);

            _dependents?.Remove(entityChange, principal);

            ((IEntityRelationshipBase<TEntity>)this).OnEntityRemoved(entityChange.Entity, principal);
        }

        protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
        {
            Debug.Assert(entityChange.RemovedComponent == null);

            if (!KeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var key))
            {
                return false;
            }

            Component componentUsed = null;
            var changes = entityChange.PropertyChanges;
            for (var i = 0; i < changes.Count; i++)
            {
                var changedComponent = changes.GetChangedComponent(i);
                if (_secondaryKeyValueGetter.PropertyUsed(changedComponent.ComponentId, changes.GetChangedPropertyName(i)))
                {
                    // The component might have been removed by the previous change listener
                    if (entityChange.Entity.FindComponent(changedComponent.ComponentId) != changedComponent)
                    {
                        return true;
                    }

                    componentUsed = changedComponent;
                    break;
                }
            }

            if (componentUsed == null)
            {
                _dependents?.PropertyValuesChanged(entityChange);

                return false;
            }

            var principal = FindPrincipal(key, entityChange.Entity, fallback: true);

            var entities = Accessor.GetOrCreateDependents(principal);

            var entity = entityChange.Entity;
            if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.PreferOld, out var oldSortKey)
                && entities.Remove(oldSortKey))
            {
                OnEntityRemoved(entityChange, principal);
            }

            if (_secondaryKeyValueGetter.TryGetKey(entityChange, ValueType.Current, out var newSortKey))
            {
                entities.Add(newSortKey, entity);
                OnEntityAdded(entityChange, principal);
            }

            return true;
        }

        public override string ToString() => "LookupRelationship: " + Name;

        private class DependentsGroup : EntityGroupBase<TEntity>
        {
            private readonly LookupEntityRelationship<TEntity, TKey, TDictionary> _relationship;

            public DependentsGroup(LookupEntityRelationship<TEntity, TKey, TDictionary> relationship)
                : base(relationship.Name)
                => _relationship = relationship;

            public override TEntity FindEntity(int id)
            {
                var entity = ((IEntityRelationshipBase<TEntity>)_relationship).FindEntity(id);
                return entity == null
                       || !_relationship._secondaryKeyValueGetter.TryGetKey(
                           new EntityChange<TEntity>(entity), ValueType.Current, out _)
                    ? null
                    : entity;
            }

            public override bool ContainsEntity(int id)
                => FindEntity(id) != null;

            public void Add(in EntityChange<TEntity> entityChange, TEntity principal)
                => OnAdded(entityChange, principal);

            public void Remove(in EntityChange<TEntity> entityChange, TEntity principal)
                => OnRemoved(entityChange, principal);

            public void PropertyValuesChanged(in EntityChange<TEntity> entityChange)
            {
                if (FindEntity(entityChange.Entity.Id) == null)
                {
                    return;
                }

                OnPropertyValuesChanged(entityChange);
            }

            public override string ToString() => "LookupDependentGroup: " + Name;
        }

        private class PrincipalGroupListener : IEntityChangeListener<TEntity>
        {
            private readonly LookupEntityRelationship<TEntity, TKey, TDictionary> _relationship;

            public PrincipalGroupListener(LookupEntityRelationship<TEntity, TKey, TDictionary> relationship)
                => _relationship = relationship;

            public void OnEntityAdded(in EntityChange<TEntity> entityChange)
            {
                var principal = entityChange.Entity;
                var key = principal.Id;
                if (!_relationship.OrphanedEntities.TryGetValue(key, out var orphanedEntities)
                    || orphanedEntities.Count == 0)
                {
                    return;
                }

                var dependents = _relationship.Accessor.GetOrCreateDependents(principal);
                foreach (var orphanedEntity in orphanedEntities)
                {
                    Debug.Assert(orphanedEntity.Manager != null);

                    var orphanedEntityChange = new EntityChange<TEntity>(orphanedEntity);
                    if (_relationship._secondaryKeyValueGetter.TryGetKey(
                        orphanedEntityChange, ValueType.Current, out var secondaryKey))
                    {
                        dependents.Add(secondaryKey, orphanedEntity);
                        _relationship.OnEntityAdded(orphanedEntityChange, principal);
                    }
                }

                _relationship.OrphanedEntities.Remove(key);
            }

            public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
            {
                var principal = entityChange.Entity;
                var dependents = _relationship.Accessor.GetDependents(principal, entityChange.RemovedComponent);
                if (dependents == null
                    || dependents.Count == 0)
                {
                    return;
                }

                Debug.Assert(_relationship.OrphanedEntities.Count == 0, $"Orphaned entities found in {_relationship.Name}");

                foreach (var dependent in dependents.Values)
                {
                    var dependentChange = new EntityChange<TEntity>(dependent);

                    _relationship.OnEntityRemoved(dependentChange, principal);
                    _relationship.HandlePrincipalEntityRemoved(dependent, entityChange);
                }
                _relationship.Accessor.ResetDependents(principal);
            }

            public bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
                => false;

            public override string ToString() => "Principal " + _relationship;
        }
    }
}
