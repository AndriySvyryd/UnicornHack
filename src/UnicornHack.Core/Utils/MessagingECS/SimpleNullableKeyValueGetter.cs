using System;
using System.Linq.Expressions;

namespace UnicornHack.Utils.MessagingECS
{
    public class SimpleNullableKeyValueGetter<TEntity, TKey> : KeyValueGetter<TEntity, TKey>
        where TEntity : Entity
        where TKey : class
    {
        public SimpleNullableKeyValueGetter(Expression<Func<Component, TKey>> getProperty, int componentId)
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

                var key = getKeyProperty(changedComponent);
                return key == null ? (default, false) : (key, true);
            };
    }
}
