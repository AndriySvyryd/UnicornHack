namespace UnicornHack.Utils.MessagingECS;

public interface IDependentEntityChangeListener<TEntity>
    where TEntity : Entity, new()
{
    void OnEntityAdded(in EntityChange<TEntity> entityChange, TEntity principal);

    void OnEntityRemoved(in EntityChange<TEntity> entityChange, TEntity principal);

    void OnPropertyValuesChanged(in EntityChange<TEntity> entityChange, TEntity principal);
}
