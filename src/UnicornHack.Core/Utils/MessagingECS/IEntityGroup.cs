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
    void AddListener(IEntityChangeListener<TEntity> index);
    TEntity? FindEntity(int id);
    bool ContainsEntity(int id);
}
