using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public interface IKeyValueGetter<TEntity, TKey> where TEntity : Entity
    {
        bool PropertyUsed(int componentId, string propertyName);
        bool TryGetKey(TEntity entity, IReadOnlyList<IPropertyValueChange> changes, bool getOldValue, out TKey keyValue);
    }
}
