namespace UnicornHack.Utils.MessagingECS;

public interface IEntityManager
{
    IMessageQueue Queue
    {
        get;
    }

    bool IsLoading
    {
        get;
    }

    ITransientReference<Entity> CreateEntity();
    void RemoveEntity(Entity entity);
    Entity? FindEntity(int id);
    Entity? LoadEntity(int id);

    TComponent CreateComponent<TComponent>(int componentId)
        where TComponent : Component, new();

    void OnComponentAdded(Component component);
    void OnComponentRemoved(Component component);

    void OnPropertyValueChanged<T>(
        string propertyName, T oldValue, T newValue, int componentId, Component component);

    void OnPropertyValuesChanged(Entity entity, IPropertyValueChanges changes);

    void RemoveFromSecondaryTracker(ITrackable trackable);
}
