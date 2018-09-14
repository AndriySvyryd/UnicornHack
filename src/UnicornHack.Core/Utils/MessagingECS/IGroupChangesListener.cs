using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IGroupChangesListener<TEntity> where TEntity : Entity
    {
        void HandleEntityAdded(TEntity entity, Component addedComponent, IEntityGroup<TEntity> group);

        void HandleEntityRemoved(TEntity entity, Component removedComponent, IEntityGroup<TEntity> group);

        bool HandlePropertyValuesChanged(IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group);
    }
}
