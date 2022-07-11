namespace UnicornHack.Utils.MessagingECS;

public class KeyValueGetter<TEntity, TKey> : IKeyValueGetter<TEntity, TKey>
    where TEntity : Entity, new()
{
    private readonly Func<EntityChange<TEntity>, PropertyMatcher<TEntity>, ValueType, (TKey, bool)> _getKey;
    private readonly PropertyMatcher<TEntity> _matcher;

    public KeyValueGetter(
        Func<EntityChange<TEntity>, PropertyMatcher<TEntity>, ValueType, (TKey, bool)> getKey,
        PropertyMatcher<TEntity> matcher)
    {
        _getKey = getKey;
        _matcher = matcher;
    }

    public bool TryGetKey(in EntityChange<TEntity> entityChange, ValueType type, out TKey keyValue)
    {
        (keyValue, var hasKey) = _getKey(entityChange, _matcher, type);
        return hasKey;
    }

    public bool PropertyUsed(int componentId, string propertyName)
        => _matcher.Matches(componentId, propertyName);
}
