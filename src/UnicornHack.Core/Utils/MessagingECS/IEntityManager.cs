using System.Collections.Generic;

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

        void HandleComponentAdded(Component component);
        void HandleComponentRemoved(Component component);

        void HandlePropertyValueChanged<T>(
            string propertyName, T oldValue, T newValue, int componentId, Component component);

        void HandlePropertyValuesChanged(IReadOnlyList<IPropertyValueChange> changes);

        void RemoveFromSecondaryTracker(ITrackable trackable);
    }
}
