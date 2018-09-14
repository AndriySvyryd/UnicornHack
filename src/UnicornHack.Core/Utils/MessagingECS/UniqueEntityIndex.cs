using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils.MessagingECS
{
    public class UniqueEntityIndex<TEntity, TKey> : EntityIndexBase<TEntity, TKey>
        where TEntity : Entity
    {
        public UniqueEntityIndex(
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter)
            : base(group, keyValueGetter)
            => Index = new Dictionary<TKey, TEntity>();

        public UniqueEntityIndex(
            IEntityGroup<TEntity> group,
            KeyValueGetter<TEntity, TKey> keyValueGetter,
            IEqualityComparer<TKey> comparer)
            : base(group, keyValueGetter)
            => Index = new Dictionary<TKey, TEntity>(comparer);

        protected Dictionary<TKey, TEntity> Index { get; }

        public TEntity this[TKey key] => Index.TryGetValue(key, out var entity) ? entity : null;

        protected override bool TryAddEntity(TKey key, TEntity entity, Component changedComponent)
        {
            if (Index.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"The key {key} already exists in the unique index for {Group.Name}");
            }

            Index[key] = entity;

            return true;
        }

        protected override bool TryRemoveEntity(TKey key, TEntity entity, Component changedComponent)
        {
            var removed = Index.Remove(key);
            Debug.Assert(removed);

            return removed;
        }
    }
}
