namespace UnicornHack.Utils.MessagingECS;

public interface IEntityGroup<TEntity>
    where TEntity : Entity, new()
{
    string Name
    {
        get;
    }

    string GetEntityAddedMessageName();
    string GetEntityRemovedMessageName();
    string GetPropertyValueChangedMessageName(string propertyName);
    string GetPropertyValueChangedMessageName(string propertyName, string nestedPropertyName);
    void AddListener(IEntityChangeListener<TEntity> index);
    void RemoveListener(IEntityChangeListener<TEntity> index);
    TEntity? FindEntity(int id);
    bool ContainsEntity(int id);
}
