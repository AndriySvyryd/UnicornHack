using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public interface IEntityGroup<TEntity> : IEnumerable<TEntity> where TEntity : Entity
    {
        string Name { get; }
        bool IsLoading { get; }
        int Count { get; }
        string GetEntityAddedMessageName();
        string GetEntityRemovedMessageName();
        string GetPropertyValueChangedMessageName(string propertyName);
        void AddListener(IGroupChangesListener<TEntity> index);
        TEntity FindEntity(int id);
        bool ContainsEntity(int id);
    }
}
