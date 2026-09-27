namespace UnicornHack.Utils.MessagingECS;

public interface IRelationship<TEntity>
    where TEntity : Entity, new()
{
    IEntityGroup<TEntity> Dependents { get; }

    TEntity? GetPrincipal(TEntity dependent, Component? removedComponent = null);

    void AddDependentsListener(IDependentEntityChangeListener<TEntity> listener);

    void RemoveDependentsListener(IDependentEntityChangeListener<TEntity> listener);
}