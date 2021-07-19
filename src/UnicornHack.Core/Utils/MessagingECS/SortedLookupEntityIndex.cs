using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedLookupEntityIndex<TEntity, TKey, TSortKey> : EntityIndexBase<TEntity, TKey>
        where TEntity : Entity
    {
        private readonly IKeyValueGetter<TEntity, TSortKey> _sortValueGetter;
        private readonly IComparer<TSortKey> _comparer;
        private static readonly IReadOnlyDictionary<TSortKey, TEntity> EmptyDictionary = new Dictionary<TSortKey, TEntity>();

        public SortedLookupEntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            IKeyValueGetter<TEntity, TKey> keyValueGetter,
            IKeyValueGetter<TEntity, TSortKey> sortValueGetter,
            IComparer<TSortKey> comparer = null)
            : base(name, group, keyValueGetter)
        {
            _sortValueGetter = sortValueGetter;
            _comparer = comparer ?? Comparer<TSortKey>.Default;
            Index = new Dictionary<TKey, SortedDictionary<TSortKey, TEntity>>();
        }

        public SortedLookupEntityIndex(
            string name,
            IEntityGroup<TEntity> group,
            IKeyValueGetter<TEntity, TKey> keyValueGetter,
            IKeyValueGetter<TEntity, TSortKey> sortValueGetter,
            IEqualityComparer<TKey> equalityComparer,
            IComparer<TSortKey> comparer = null)
            : base(name, group, keyValueGetter)
        {
            _sortValueGetter = sortValueGetter;
            _comparer = comparer ?? Comparer<TSortKey>.Default;
            Index = new Dictionary<TKey, SortedDictionary<TSortKey, TEntity>>(equalityComparer);
        }

        protected Dictionary<TKey, SortedDictionary<TSortKey, TEntity>> Index { get; }

        public IReadOnlyDictionary<TSortKey, TEntity> this[TKey key]
            => Index.TryGetValue(key, out var entities) ? entities : EmptyDictionary;

        private SortedDictionary<TSortKey, TEntity> GetOrAddEntities(TKey key)
        {
            if (!Index.TryGetValue(key, out var entities))
            {
                entities = new SortedDictionary<TSortKey, TEntity>(_comparer);
                Index.Add(key, entities);
            }

            return entities;
        }

        protected override bool TryAddEntity(TKey key, TEntity entity, Component changedComponent)
        {
            if (_sortValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false, out var sortKey))
            {
                GetOrAddEntities(key).Add(sortKey, entity);

                return true;
            }

            return false;
        }

        protected override bool TryRemoveEntity(TKey key, TEntity entity, Component changedComponent)
        {
            if (!Index.TryGetValue(key, out var entities))
            {
                return false;
            }

            if (!_sortValueGetter.TryGetKey(entity, new IPropertyValueChange[0], getOldValue: false, out var sortKey))
            {
                sortKey = entities.Where(p => p.Value == entity).Select(p => p.Key).FirstOrDefault();
            }

            if (!Equals(sortKey, default)
                && entities.Remove(sortKey))
            {
                if (entities.Count == 0)
                {
                    Index.Remove(key);
                }

                return true;
            }

            return false;
        }

        public override bool HandlePropertyValuesChanged(
            IReadOnlyList<IPropertyValueChange> changes, TEntity entity, IEntityGroup<TEntity> group)
        {
            if (base.HandlePropertyValuesChanged(changes, entity, group))
            {
                return true;
            }

            Component componentUsed = null;
            for (var i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                if (_sortValueGetter.PropertyUsed(change.ChangedComponent.ComponentId, change.ChangedPropertyName))
                {
                    componentUsed = change.ChangedComponent;
                    break;
                }

                // The component might have been removed by the previous change listener
                if (!entity.HasComponent(change.ChangedComponent.ComponentId))
                {
                    return true;
                }
            }

            if (componentUsed == null)
            {
                return false;
            }

            if (!KeyValueGetter.TryGetKey(entity, changes, getOldValue: false, out var key))
            {
                return true;
            }

            var entities = GetOrAddEntities(key);

            if (_sortValueGetter.TryGetKey(entity, changes, getOldValue: true, out var oldSortKey))
            {
                entities.Remove(oldSortKey);
            }

            if (_sortValueGetter.TryGetKey(entity, changes, getOldValue: false, out var newSortKey))
            {
                entities.Add(newSortKey, entity);
            }

            if (entities.Count == 0)
            {
                Index.Remove(key);
            }

            return true;
        }

        public override string ToString() => "SortedLookupIndex: " + Name;

        public override IEnumerator<TEntity> GetEnumerator()
            => Index.Values.SelectMany(v => v.Values).GetEnumerator();
    }
}
