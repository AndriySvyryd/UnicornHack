using System.Collections;

// ReSharper disable StaticMemberInGenericType
namespace UnicornHack.Utils.MessagingECS;

public class EntityGroup<TEntity> : EntityGroupBase<TEntity>, IEnumerable<TEntity>
    where TEntity : Entity, new()
{
    private readonly Dictionary<int, TEntity> _entities = new();

    public EntityGroup(string name, EntityMatcher<TEntity> matcher)
        : base(name)
    {
        Matcher = matcher;
    }

    public EntityMatcher<TEntity> Matcher
    {
        get;
    }

    public int Count => _entities.Count;

    public override TEntity? FindEntity(int id) => _entities.TryGetValue(id, out var entity) ? entity : null;
    public override bool ContainsEntity(int id) => _entities.ContainsKey(id);

    public void HandleEntityComponentChanged(TEntity entity, Component? removedComponent)
    {
        if (!Matcher.Matches(entity))
        {
            if (_entities.Remove(entity.Id))
            {
                OnRemoved(new EntityChange<TEntity>(entity, removedComponent: removedComponent), principal: null);
            }
        }
        else
        {
            if (_entities.TryAdd(entity.Id, entity))
            {
                OnAdded(new EntityChange<TEntity>(entity, removedComponent: removedComponent), principal: null);
            }
        }
    }

    public void HandlePropertyValuesChanged(in EntityChange<TEntity> entityChange)
    {
        if (Matcher.Matches(entityChange.Entity))
        {
            OnPropertyValuesChanged(entityChange);
        }
    }

    public IEnumerator<TEntity> GetEnumerator() => _entities.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => "EntityGroup: " + Name;
}
