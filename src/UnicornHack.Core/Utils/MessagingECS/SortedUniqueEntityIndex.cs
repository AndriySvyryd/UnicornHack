using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedUniqueEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>, IEnumerable<TEntity>
        where TEntity : Entity
    {
        public SortedUniqueEntityIndex(
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter)
            : base(group, keyValueGetter)
            => Index = new SortedDictionary<TKey, TEntity>();

        public SortedUniqueEntityIndex(
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter,
            IComparer<TKey> comparer)
            : base(group, keyValueGetter)
            => Index = new SortedDictionary<TKey, TEntity>(comparer);

        protected SortedDictionary<TKey, TEntity> Index { get; }

        public TEntity this[TKey key] => Index.TryGetValue(key, out var entity) ? entity : null;

        protected override bool TryAddEntity(TKey key, TEntity entity, Component changedComponent)
        {
            Debug.Assert(!Index.TryGetValue(key, out var _));

            Index[key] = entity;

            return true;
        }

        protected override bool TryRemoveEntity(TKey key, TEntity entity, Component changedComponent)
        {
            Debug.Assert(Index.TryGetValue(key, out var _));

            return Index.Remove(key);
        }

        public TEntity First() => Index.First().Value;

        public TEntity Last() => Index.Last().Value;

        public int Count => Index.Count;

        public IEnumerator<TEntity> GetEnumerator() => Index.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
