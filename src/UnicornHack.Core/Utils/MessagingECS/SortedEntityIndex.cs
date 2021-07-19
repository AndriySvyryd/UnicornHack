using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>
        where TEntity : Entity
    {
        private readonly IComparer<TEntity> _comparer;

        public SortedEntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter,
            IComparer<TEntity> comparer)
            : base(name, group, keyValueGetter)
        {
            _comparer = comparer;
            Index = new Dictionary<TKey, SortedSet<TEntity>>();
        }

        public SortedEntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter,
            IEqualityComparer<TKey> equalityComparer,
            IComparer<TEntity> comparer)
            : base(name, group, keyValueGetter)
        {
            _comparer = comparer;
            Index = new Dictionary<TKey, SortedSet<TEntity>>(equalityComparer);
        }

        protected Dictionary<TKey, SortedSet<TEntity>> Index { get; }

        public IEnumerable<TEntity> this[TKey key]
            => Index.TryGetValue(key, out var entities) ? entities : Enumerable.Empty<TEntity>();

        private SortedSet<TEntity> GetOrAddEntities(TKey key)
        {
            if (!Index.TryGetValue(key, out var entities))
            {
                entities = new SortedSet<TEntity>(_comparer);
                Index.Add(key, entities);
            }

            return entities;
        }

        protected override bool TryAddEntity(TKey key, TEntity entity, Component changedComponent)
            => GetOrAddEntities(key).Add(entity);

        protected override bool TryRemoveEntity(TKey key, TEntity entity, Component changedComponent)
            => GetOrAddEntities(key).Remove(entity);

        public override string ToString() => "SortedIndex: " + Name;

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.SelectMany(v => v).GetEnumerator();
    }
}
