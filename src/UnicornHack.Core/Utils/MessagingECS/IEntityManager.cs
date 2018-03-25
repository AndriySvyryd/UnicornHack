namespace UnicornHack.Utils.MessagingECS
{
    public interface IEntityManager
    {
        IMessageQueue Queue { get; }

        ITransientReference<Entity> CreateEntity();
        void RemoveEntity(Entity entity);
        Entity FindEntity(int id);
        Entity LoadEntity(int id);

        TComponent CreateComponent<TComponent>(int componentId)
            where TComponent : Component, new();

        void HandleComponentAdded(int id, Component component);
        void HandleComponentRemoved(int id, Component component);

        void HandlePropertyValueChanged<T>(
            string propertyName, T oldValue, T newValue, int componentId, Component component);

        void RemoveFromSecondaryTracker(ITrackable trackable);
    }
}
