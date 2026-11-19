namespace UnicornHack.Utils.MessagingECS;

public class EntityMatcher<TEntity> where TEntity : Entity
{
    private readonly SortedSet<int> _allIds = [];
    private uint _allOfMask;
    private uint? _anyOfMask;
    private uint _noneOfMask;

    public EntityMatcher<TEntity> AllOf(params int[] ids)
    {
        _allOfMask |= Entity.GetComponentMask(ids);
        _allIds.UnionWith(ids);
        return this;
    }

    public EntityMatcher<TEntity> AnyOf(params int[] ids)
    {
        _anyOfMask = (_anyOfMask ?? 0) | Entity.GetComponentMask(ids);
        _allIds.UnionWith(ids);
        return this;
    }

    public EntityMatcher<TEntity> NoneOf(params int[] ids)
    {
        _noneOfMask |= Entity.GetComponentMask(ids);
        _allIds.UnionWith(ids);
        return this;
    }

    public bool Matches(TEntity entity)
    {
        var mask = entity.ComponentMask;
        return (mask & _noneOfMask) == 0
               && (mask & _allOfMask) == _allOfMask
               && (_anyOfMask is not { } anyOfMask || (mask & anyOfMask) != 0);
    }

    public IEnumerable<int> GetAllIds() => _allIds;
}
