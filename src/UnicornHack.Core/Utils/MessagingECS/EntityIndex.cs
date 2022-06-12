using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class EntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>
        where TEntity : Entity, new()
    {
        public EntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            IKeyValueGetter<TEntity, TKey> keyValueGetter)
            : base(name, group, keyValueGetter)
            => Index = new Dictionary<TKey, HashSet<TEntity>>();

        public EntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            IKeyValueGetter<TEntity, TKey> keyValueGetter,
            IEqualityComparer<TKey> comparer)
            : base(name, group, keyValueGetter)
            => Index = new Dictionary<TKey, HashSet<TEntity>>(comparer);

        protected Dictionary<TKey, HashSet<TEntity>> Index { get; }

        public IEnumerable<TEntity> this[TKey key]
            => Index.TryGetValue(key, out var entities) ? entities : Enumerable.Empty<TEntity>();

        private HashSet<TEntity> GetOrAddEntities(TKey key)
        {
            if (!Index.TryGetValue(key, out var entities))
            {
                entities = new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance);
                Index.Add(key, entities);
            }

            return entities;
        }

        protected override bool TryAddEntity(TKey key, in EntityChange<TEntity> change)
            => GetOrAddEntities(key).Add(change.Entity);

        protected override bool TryRemoveEntity(TKey key, in EntityChange<TEntity> change)
            => GetOrAddEntities(key).Remove(change.Entity);

        protected override bool HandleNonKeyPropertyValuesChanged(in EntityChange<TEntity> entityChange)
            => false;

        public override string ToString() => "Index: " + Name;
    }
}
