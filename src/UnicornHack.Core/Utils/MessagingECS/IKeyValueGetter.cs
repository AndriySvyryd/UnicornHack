namespace UnicornHack.Utils.MessagingECS;

public interface IKeyValueGetter<TEntity, TKey>
    where TEntity : Entity, new()
{
    bool PropertyUsed(int componentId, string propertyName);

    bool TryGetKey(in EntityChange<TEntity> entityChange, ValueType type, out TKey keyValue);
}
