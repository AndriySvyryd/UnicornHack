using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS;

public class SimpleKeyValueGetter<TEntity, TKey> : IKeyValueGetter<TEntity, TKey>
    where TEntity : Entity, new()
    where TKey : struct
{
    private readonly string _propertyName;
    private readonly int _componentId;
    private readonly bool _nullable;
    private readonly PropertyMatcher<TEntity> _matcher;

    public SimpleKeyValueGetter(Expression<Func<Component, TKey>> getProperty, int componentId)
    {
        _propertyName = getProperty.GetSimplePropertyAccess().Name;
        _componentId = componentId;
        _nullable = false;
        _matcher = new PropertyMatcher<TEntity>().With(getProperty, componentId);
    }

    public SimpleKeyValueGetter(Expression<Func<Component, TKey?>> getProperty, int componentId)
    {
        _propertyName = getProperty.GetSimplePropertyAccess().Name;
        _componentId = componentId;
        _nullable = true;
        _matcher = new PropertyMatcher<TEntity>().With(getProperty, componentId);
    }

    public bool TryGetKey(
        in EntityChange<TEntity> entityChange,
        ValueType type,
        out TKey keyValue)
    {
        if (_nullable)
        {
            if (_matcher.TryGetValue<TKey?>(entityChange, _componentId, _propertyName, type, out var value)
                && value.HasValue)
            {
                keyValue = value.Value;
                return true;
            }
        }
        else if (_matcher.TryGetValue<TKey>(entityChange, _componentId, _propertyName, type, out var value)
                 && !EqualityComparer<TKey>.Default.Equals(value, default))
        {
            keyValue = value;
            return true;
        }

        keyValue = default;
        return false;
    }

    public bool PropertyUsed(int componentId, string propertyName)
        => _matcher.Matches(componentId, propertyName);
}
