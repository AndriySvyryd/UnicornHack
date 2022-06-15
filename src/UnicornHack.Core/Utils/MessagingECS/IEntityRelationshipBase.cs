namespace UnicornHack.Utils.MessagingECS;

public interface IEntityRelationshipBase<TEntity>
    where TEntity : Entity, new()
{
    string Name
    {
        get;
    }

    void OnEntityAdded(TEntity dependent, TEntity principal);
    void OnEntityRemoved(TEntity dependent, TEntity principal);

    /// <summary>
    ///     Tries to find a referencing entity with the given <paramref name="id" />.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    TEntity FindEntity(int id);

    /// <summary>
    ///     Returns <c>true</c> if there is a referencing entity with the given <paramref name="id" />.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool ContainsEntity(int id);
}
