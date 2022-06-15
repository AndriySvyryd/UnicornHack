using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

public class UniqueEntityRelationship<TEntity> : EntityRelationshipBase<TEntity>
    where TEntity : Entity, new()
{
    private DependentsGroup _dependents;

    public UniqueEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Expression<Func<TEntity, TEntity>> getDependent = null,
        Expression<Func<TEntity, TEntity>> getPrincipal = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        Accessor = getDependent == null
            ? new ExternalReferenceAccessor<TEntity>()
            : new PropertyReferenceAccessor<TEntity>(getDependent, getPrincipal);
    }

    public UniqueEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Func<TEntity, TEntity> getDependent,
        Action<TEntity, TEntity> setDependent,
        Func<Component, TEntity> componentGetDependent,
        Func<TEntity, TEntity> getPrincipal = null,
        Action<TEntity, TEntity> setPrincipal = null,
        Func<Component, TEntity> componentGetPrincipal = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            name,
            dependentGroup,
            principalGroup,
            keyValueGetter,
            handlePrincipalDeleted,
            keepPrincipalAlive,
            keepDependentAlive)
    {
        Accessor = new PropertyReferenceAccessor<TEntity>(
            getDependent, setDependent, componentGetDependent, getPrincipal, setPrincipal, componentGetPrincipal);
    }

    private UniqueEntityRelationship(
        string name,
        IEntityGroup<TEntity> dependentGroup,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, int> keyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        bool keepPrincipalAlive,
        bool keepDependentAlive)
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
    }

    protected IReferenceAccessor<TEntity> Accessor
    {
        get;
    }

    protected Dictionary<int, TEntity> OrphanedEntities
    {
        get;
    } = new();

    public IEntityGroup<TEntity> Dependents
        => _dependents ??= new DependentsGroup(this);

    public TEntity GetDependent(TEntity principal, Component removedComponent = null)
        => Accessor.GetDependent(principal, removedComponent);

    public TEntity GetPrincipal(TEntity dependent, Component removedComponent = null)
        => Accessor.TryGetPrincipal(dependent, out var principal, removedComponent)
            ? principal
            : KeyValueGetter.TryGetKey(new EntityChange<TEntity>(dependent), ValueType.Current, out var key)
                ? FindPrincipal(key, dependent, fallback: false)
                : null;

    protected override bool TryAddEntity(int key, in EntityChange<TEntity> entityChange)
    {
        var referenced = FindPrincipal(key, entityChange.Entity, fallback: false);
        if (referenced != null)
        {
            Add(key, entityChange, referenced);
            return true;
        }

        OrphanedEntities.Add(key, entityChange.Entity);

        return false;
    }

    private void Add(int key, in EntityChange<TEntity> entityChange, TEntity principal)
    {
        if (Accessor.GetDependent(principal) != null)
        {
            throw new InvalidOperationException(
                $"The key {key} already exists in the unique relationship for {_dependents?.Name}");
        }

        Accessor.SetDependent(principal, entityChange.Entity);
        Accessor.SetPrincipal(entityChange.Entity, principal);

        _dependents?.Add(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityAdded(entityChange.Entity, principal);
    }

    protected override bool TryRemoveEntity(int key, in EntityChange<TEntity> entityChange)
    {
        var principal = FindPrincipal(key, entityChange.Entity, fallback: true);
        if (principal == null)
        {
            return false;
        }

        if (Accessor.GetDependent(principal) != null)
        {
            Remove(entityChange, principal);
            return true;
        }

        Debug.Assert(OrphanedEntities.Count == 0, $"Orphaned entities found in {Name}");

        return false;
    }

    private void Remove(in EntityChange<TEntity> entityChange, TEntity principal)
    {
        Accessor.SetDependent(principal, null);
        Accessor.SetPrincipal(entityChange.Entity, null);

        _dependents?.Remove(entityChange, principal);

        ((IEntityRelationshipBase<TEntity>)this).OnEntityRemoved(entityChange.Entity, principal);
    }

    protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        _dependents?.PropertyValuesChanged(entityChange);

        return true;
    }

    public override string ToString() => "UniqueRelationship: " + Name;

    private class DependentsGroup : EntityGroupBase<TEntity>
    {
        private readonly IEntityRelationshipBase<TEntity> _relationship;

        public DependentsGroup(IEntityRelationshipBase<TEntity> relationship)
            : base(relationship.Name)
        {
            _relationship = relationship;
        }

        public override TEntity FindEntity(int id) => _relationship.FindEntity(id);

        public override bool ContainsEntity(int id) => _relationship.ContainsEntity(id);

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

        public override string ToString() => "ReferenceDependentGroup: " + Name;
    }

    private class PrincipalGroupListener : IEntityChangeListener<TEntity>
    {
        private readonly UniqueEntityRelationship<TEntity> _relationship;

        public PrincipalGroupListener(UniqueEntityRelationship<TEntity> relationship)
        {
            _relationship = relationship;
        }

        public void OnEntityAdded(in EntityChange<TEntity> entityChange)
        {
            var key = entityChange.Entity.Id;
            if (!_relationship.OrphanedEntities.TryGetValue(key, out var orphanedEntity))
            {
                return;
            }

            Debug.Assert(orphanedEntity.Manager != null);

            _relationship.OrphanedEntities.Remove(key);
            _relationship.Add(key, new EntityChange<TEntity>(orphanedEntity), entityChange.Entity);
        }

        public void OnEntityRemoved(in EntityChange<TEntity> entityChange)
        {
            var principal = entityChange.Entity;
            var dependent = _relationship.Accessor.GetDependent(principal, entityChange.RemovedComponent);
            if (dependent == null)
            {
                return;
            }

            Debug.Assert(_relationship.OrphanedEntities.Count == 0,
                $"Orphaned entities found in {_relationship.Name}");

            var dependentChange = new EntityChange<TEntity>(dependent);

            _relationship.Remove(dependentChange, entityChange.Entity);
            _relationship.HandlePrincipalEntityRemoved(dependent, entityChange);
        }

        public bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange)
            => false;

        public override string ToString() => "Principal " + _relationship;
    }
}
