using System;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class SimpleNonNullableKeyValueGetter<TEntity, TKey> : KeyValueGetter<TEntity, TKey>
        where TEntity : Entity
        where TKey : struct
    {
        public SimpleNonNullableKeyValueGetter(Expression<Func<Component, TKey>> getProperty, int componentId)
            : base(CreateSimpleKeyGetter(getProperty.Compile(), componentId),
                new PropertyMatcher(componentId, getProperty.GetPropertyAccess().Name))
        {
        }

        public SimpleNonNullableKeyValueGetter(Expression<Func<Component, TKey?>> getProperty, int componentId)
            : base(CreateSimpleKeyGetter(getProperty.Compile(), componentId),
                new PropertyMatcher(componentId, getProperty.GetPropertyAccess().Name))
        {
        }

        private static Func<TEntity, int, Component, string, object, (TKey, bool)> CreateSimpleKeyGetter(
            Func<Component, TKey> getKeyProperty, int componentId)
            => (entity, changedComponentId, changedComponent, changedProperty, changedValue) =>
            {
                if (changedComponentId == componentId)
                {
                    if (changedProperty != null)
                    {
                        return ((TKey)changedValue, true);
                    }
                }
                else
                {
                    changedComponent = entity.FindComponent(componentId);
                }

                return (getKeyProperty(changedComponent), true);
            };

        private static Func<TEntity, int, Component, string, object, (TKey, bool)> CreateSimpleKeyGetter(
            Func<Component, TKey?> getKeyProperty, int componentId)
            => (entity, changedComponentId, changedComponent, changedProperty, changedValue) =>
            {
                TKey? key;
                if (changedComponentId == componentId)
                {
                    if (changedProperty != null)
                    {
                        key = (TKey?)changedValue;
                    }
                    else
                    {
                        key = getKeyProperty(changedComponent);
                    }
                }
                else
                {
                    key = getKeyProperty(entity.FindComponent(componentId));
                }

                return key == null ? (default, false) : (key.Value, true);
            };
    }
}
