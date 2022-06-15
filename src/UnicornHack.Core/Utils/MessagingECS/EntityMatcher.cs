using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS;

public class EntityMatcher<TEntity> where TEntity : Entity
{
    private int[] _allOfIds;
    private int[] _anyOfIds;
    private int[] _noneOfIds;

    public EntityMatcher<TEntity> AllOf(params int[] ids)
    {
        _allOfIds = Merge(_allOfIds, ids);
        return this;
    }

    public EntityMatcher<TEntity> AnyOf(params int[] ids)
    {
        _anyOfIds = Merge(_anyOfIds, ids);
        return this;
    }

    public EntityMatcher<TEntity> NoneOf(params int[] ids)
    {
        _noneOfIds = Merge(_noneOfIds, ids);
        return this;
    }

    public bool Matches(TEntity entity)
        => (_noneOfIds == null || !entity.HasAnyComponent(_noneOfIds))
           && (_allOfIds == null || entity.HasComponents(_allOfIds))
           && (_anyOfIds == null || entity.HasAnyComponent(_anyOfIds));

    public IEnumerable<int> GetAllIds()
        => (_allOfIds ?? Enumerable.Empty<int>())
            .Concat(_anyOfIds ?? Enumerable.Empty<int>())
            .Concat(_noneOfIds ?? Enumerable.Empty<int>())
            .Distinct();

    private static int[] Merge(int[] firstArray, int[] secondArray)
        => firstArray?.Concat(secondArray).Distinct().OrderBy(id => id).ToArray()
           ?? secondArray;
}
