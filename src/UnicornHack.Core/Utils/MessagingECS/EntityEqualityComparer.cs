using System.Collections.Generic;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityEqualityComparer<TEntity> : IEqualityComparer<TEntity>
        where TEntity : Entity
    {
        private EntityEqualityComparer()
        {
        }

        public static readonly IEqualityComparer<TEntity> Instance = new EntityEqualityComparer<TEntity>();

        public bool Equals(TEntity x, TEntity y) => x.Id == y.Id;

        public int GetHashCode(TEntity obj) => obj.Id;
    }
}
