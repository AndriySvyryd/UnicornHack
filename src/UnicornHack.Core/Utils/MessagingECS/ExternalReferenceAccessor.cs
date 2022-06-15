using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS;

public class ExternalReferenceAccessor<TEntity> : IReferenceAccessor<TEntity>
    where TEntity : Entity, new()
{
    protected Dictionary<int, TEntity> Index
    {
        get;
    } = new();

    public TEntity GetDependent(TEntity principal, Component removedComponent = null)
        => Index.TryGetValue(principal.Id, out var dependent)
            ? dependent
            : null;

    public void SetDependent(TEntity principal, TEntity dependent)
        => Index[principal.Id] = dependent;

    public void SetPrincipal(TEntity dependent, TEntity principal)
    {
    }

    public bool TryGetPrincipal(TEntity dependent, out TEntity principal, Component removedComponent = null)
    {
        principal = null;
        return false;
    }
}
