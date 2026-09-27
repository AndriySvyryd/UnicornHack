using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

/// <summary>
///     A lookup relationship whose primary membership and principal are based on another relationship.
/// </summary>
public class SkipLookupEntityRelationship<TEntity, TKey, TDictionary> :
    LookupEntityRelationship<TEntity, TKey, TDictionary>,
    IDependentEntityChangeListener<TEntity>
    where TEntity : Entity, new()
    where TKey : notnull
    where TDictionary : class, IDictionary<TKey, TEntity>, new()
{
    private sealed class PrincipalKeys
    {
        public Dictionary<int, int> Values { get; } = [];
    }

    private readonly PrincipalKeys _principalKeys;

    public SkipLookupEntityRelationship(
        string name,
        IRelationship<TEntity> containingRelationship,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Expression<Func<TEntity, TDictionary?>>? getDependent = null,
        Func<TDictionary>? factory = null,
        bool keepPrincipalAlive = false,
        bool keepDependentAlive = false)
        : this(
            new PrincipalKeys(), name, containingRelationship, principalGroup,
            secondaryKeyValueGetter, handlePrincipalDeleted, getDependent, factory,
            keepPrincipalAlive, keepDependentAlive)
    {
    }

    private SkipLookupEntityRelationship(
        PrincipalKeys principalKeys,
        string name,
        IRelationship<TEntity> containingRelationship,
        IEntityGroup<TEntity> principalGroup,
        IKeyValueGetter<TEntity, TKey> secondaryKeyValueGetter,
        Action<TEntity, EntityChange<TEntity>> handlePrincipalDeleted,
        Expression<Func<TEntity, TDictionary?>>? getDependent,
        Func<TDictionary>? factory,
        bool keepPrincipalAlive,
        bool keepDependentAlive)
        : base(
            name,
            containingRelationship.Dependents,
            principalGroup,
            new KeyValueGetter<TEntity, int>(
                (change, _, _) => principalKeys.Values.TryGetValue(change.Entity.Id, out var principalId)
                    ? (principalId, true)
                    : (0, false),
                new PropertyMatcher<TEntity>()),
            secondaryKeyValueGetter,
            handlePrincipalDeleted,
            getDependent,
            factory: factory,
            keepPrincipalAlive: keepPrincipalAlive,
            keepDependentAlive: keepDependentAlive)
    {
        _principalKeys = principalKeys;
        containingRelationship.AddDependentsListener(this);
    }

    void IDependentEntityChangeListener<TEntity>.OnEntityAdded(
        in EntityChange<TEntity> entityChange, TEntity principal)
    {
        _principalKeys.Values[entityChange.Entity.Id] = principal.Id;
        TryAddEntity(principal.Id, entityChange);
    }

    void IDependentEntityChangeListener<TEntity>.OnEntityRemoved(
        in EntityChange<TEntity> entityChange, TEntity principal)
        => _principalKeys.Values.Remove(entityChange.Entity.Id);

    void IDependentEntityChangeListener<TEntity>.OnPropertyValuesChanged(
        in EntityChange<TEntity> entityChange, TEntity principal)
    {
    }
}