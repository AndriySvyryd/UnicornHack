using System;

namespace UnicornHack.Utils.MessagingECS
{
    public class KeyValueGetter<TEntity, TKey>
        where TEntity : Entity
    {
        private readonly PropertyMatcher _matcher;
        private readonly Func<TEntity, int, Component, string, object, (TKey, bool)> _getKey;

        public KeyValueGetter(
            Func<TEntity, int, Component, string, object, (TKey, bool)> getKey,
            PropertyMatcher matcher)
        {
            _getKey = getKey;
            _matcher = matcher;
        }

        public bool TryGetKey(
            TEntity entity, int componentId, Component component, string propertyName, object propertyValue,
            out TKey keyValue)
        {
            bool hasKey;
            (keyValue, hasKey) = _getKey(entity, componentId, component, propertyName, propertyValue);
            return hasKey;
        }

        public bool PropertyUsed(int componentId, string propertyName)
            => _matcher.Matches(componentId, propertyName);
    }
}
