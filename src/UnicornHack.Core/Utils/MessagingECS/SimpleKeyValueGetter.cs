using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class SimpleKeyValueGetter<TEntity, TKey> : IKeyValueGetter<TEntity, TKey>
        where TEntity : Entity
        where TKey : struct
    {
        private readonly string _propertyName;
        private readonly int _componentId;
        private readonly bool _nullable;
        private readonly PropertyMatcher _matcher;

        public SimpleKeyValueGetter(Expression<Func<Component, TKey>> getProperty, int componentId)
        {
            _propertyName = getProperty.GetPropertyAccess().Name;
            _componentId = componentId;
            _nullable = false;
            _matcher = new PropertyMatcher().With(getProperty, componentId);
        }

        public SimpleKeyValueGetter(Expression<Func<Component, TKey?>> getProperty, int componentId)
        {
            _propertyName = getProperty.GetPropertyAccess().Name;
            _componentId = componentId;
            _nullable = true;
            _matcher = new PropertyMatcher().With(getProperty, componentId);
        }

        public bool TryGetKey(TEntity entity, IReadOnlyList<IPropertyValueChange> changes, bool getOldValue, out TKey keyValue)
        {
            if (_nullable)
            {
                if (_matcher.TryGetValue<TKey?>(entity, _componentId, _propertyName, changes, getOldValue, out var value)
                    && value.HasValue)
                {
                    keyValue = value.Value;
                    return true;
                }
            }
            else if (_matcher.TryGetValue<TKey>(entity, _componentId, _propertyName, changes, getOldValue, out var value))
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
}
