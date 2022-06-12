namespace UnicornHack.Utils.MessagingECS
{
    public interface IReferenceAccessor<TEntity>
        where TEntity : Entity, new()
    {
        TEntity GetDependent(TEntity principal, Component removedComponent = null);
        void SetDependent(TEntity principal, TEntity dependent);
        void SetPrincipal(TEntity dependent, TEntity principal);
        bool TryGetPrincipal(TEntity dependent, out TEntity principal, Component removedComponent = null);
    }
}
