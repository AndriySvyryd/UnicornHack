using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Hubs
{
    public class GameTransmissionProtocol
    {
        public List<object> Serialize(
            GameEntity playerEntity, EntityState state, GameSnapshot snapshot, SerializationContext context)
            => GameSnapshot.Serialize(playerEntity, state, snapshot, context);

        public static List<List<object>> Serialize<T>(
            ISnapshotableCollection<T> collection,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            SerializationContext context)
            where T : class
            => Serialize(collection, collection.Snapshot, serializeElement, context);

        // This is a destructive operation on the snapshot
        public static List<List<object>> Serialize<T>(
            IEnumerable<T> collection,
            HashSet<T> snapshot,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            SerializationContext context)
            where T : class
        {
            var removed = snapshot ?? new HashSet<T>();
            var serializedElements = new List<List<object>>();

            foreach (var element in collection)
            {
                if (!removed.Remove(element))
                {
                    serializedElements.Add(serializeElement(element, EntityState.Added, context));

                    continue;
                }

                var serialized = serializeElement(element, EntityState.Modified, context);
                if (serialized != null)
                {
                    serializedElements.Add(serialized);
                }
            }

            foreach (var element in removed)
            {
                serializedElements.Add(serializeElement(element, EntityState.Deleted, context));
            }

            return serializedElements;
        }

        // This is a destructive operation on the snapshot
        public static List<List<object>> Serialize<TElement, TSnapshot>(
            IEnumerable<TElement> collection,
            Dictionary<TElement, TSnapshot> snapshots,
            Func<TElement, EntityState?, TSnapshot, SerializationContext, List<object>> serializeElement,
            SerializationContext context)
            where TElement : GameEntity
            where TSnapshot : class
        {
            var removed = snapshots ?? new Dictionary<TElement, TSnapshot>();
            var serializedElements = new List<List<object>>();

            foreach (var element in collection)
            {
                if (!removed.TryGetValue(element, out var snapshot))
                {
                    serializedElements.Add(serializeElement(element, EntityState.Added, null, context));

                    continue;
                }

                removed.Remove(element);
                var serialized = serializeElement(element, EntityState.Modified, snapshot, context);
                if (serialized != null)
                {
                    serializedElements.Add(serialized);
                }
            }

            foreach (var pair in removed)
            {
                serializedElements.Add(serializeElement(pair.Key, EntityState.Deleted, pair.Value, context));
            }

            return serializedElements;
        }
    }
}
