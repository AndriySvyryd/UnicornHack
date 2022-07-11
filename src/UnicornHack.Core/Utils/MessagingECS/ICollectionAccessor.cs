namespace UnicornHack.Utils.MessagingECS;

public interface ICollectionAccessor<TEntity, TCollection, TElement>
    where TEntity : Entity, new()
    where TCollection : class, ICollection<TElement>
{
    TCollection? GetDependents(TEntity principal, Component? removedComponent = null);
    TCollection GetOrCreateDependents(TEntity principal);
    void ResetDependents(TEntity principal);
    void SetPrincipal(TEntity dependent, TEntity? principal);
    bool TryGetPrincipal(TEntity dependent, out TEntity? principal, Component? removedComponent = null);
    void SetDefaultFactory(Func<TCollection> factory);
}
