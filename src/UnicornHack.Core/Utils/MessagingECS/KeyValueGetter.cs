using System;
using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public class KeyValueGetter<TEntity, TKey> : IKeyValueGetter<TEntity, TKey>
        where TEntity : Entity
    {
        private readonly Func<TEntity, IReadOnlyList<IPropertyValueChange>, bool, PropertyMatcher, (TKey, bool)>
            _getKey;

        private readonly PropertyMatcher _matcher;

        public KeyValueGetter(
            Func<TEntity, IReadOnlyList<IPropertyValueChange>, bool, PropertyMatcher, (TKey, bool)> getKey,
            PropertyMatcher matcher)
        {
            _getKey = getKey;
            _matcher = matcher;
        }

        public bool TryGetKey(TEntity entity, IReadOnlyList<IPropertyValueChange> changes, bool getOldValue,
            out TKey keyValue)
        {
            bool hasKey;
            (keyValue, hasKey) = _getKey(entity, changes, getOldValue, _matcher);
            return hasKey;
        }

        public bool PropertyUsed(int componentId, string propertyName)
            => _matcher.Matches(componentId, propertyName);
    }
}
