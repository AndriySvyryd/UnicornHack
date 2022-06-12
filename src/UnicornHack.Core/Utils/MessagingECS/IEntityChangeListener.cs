namespace UnicornHack.Utils.MessagingECS
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IEntityChangeListener<TEntity>
        where TEntity : Entity, new()
    {
        void OnEntityAdded(in EntityChange<TEntity> entityChange);

        void OnEntityRemoved(in EntityChange<TEntity> entityChange);

        bool OnPropertyValuesChanged(in EntityChange<TEntity> entityChange);
    }
}
