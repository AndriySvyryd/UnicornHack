using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnicornHack.Utils;

namespace UnicornHack.Data
{
    public static class CollectionChanges
    {
        public static List<List<object>> SerializeCollection<T>(
            ISnapshotableCollection<T> collection,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            SerializationContext context)
            where T : class
            => SerializeCollection(collection, serializeElement, (T _) => true, context);

        public static List<List<object>> SerializeCollection<T>(
            ISnapshotableCollection<T> collection,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            Func<T, bool> predicate,
            SerializationContext context)
            where T : class
            => SerializeCollection(collection,
                serializeElement,
                (_, c) => c,
                (_, c) => c,
                (e, _, p) => p(e),
                context,
                predicate);

        public static List<List<object>> SerializeCollection<T>(
            ISnapshotableCollection<T> collection,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            Func<PropertyValues, bool> predicate,
            SerializationContext context)
            where T : class
            => SerializeCollection<T, Func<T, PropertyValues>, Func<PropertyValues, bool>>(collection,
                serializeElement,
                (_, c) => e => c.Context.Entry(e).OriginalValues,
                (_, c) => e => c.Context.Entry(e).CurrentValues,
                (e, getValues, p) => p(getValues(e)),
                context,
                predicate);

        public static List<List<object>> SerializeCollection<T, TState, TAdditionalState>(
            ISnapshotableCollection<T> collection,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            Func<IEnumerable<T>, SerializationContext, TState> createPreviousState,
            Func<IEnumerable<T>, SerializationContext, TState> createCurrentState,
            Func<T, TState, TAdditionalState, bool> predicate,
            SerializationContext context,
            TAdditionalState additionalState)
            where T : class
        {
            var removed = collection.Snapshot == null ? new HashSet<T>() : new HashSet<T>(collection.Snapshot);
            var serializedElements = new List<List<object>>();
            var previousState = createPreviousState(removed, context);
            var currentState = createCurrentState(collection, context);

            foreach (var element in collection)
            {
                if (!removed.Remove(element))
                {
                    if (predicate(element, currentState, additionalState))
                    {
                        serializedElements.Add(serializeElement(element, EntityState.Added, context));
                    }
                    continue;
                }

                EntityState state;
                if (predicate(element, previousState, additionalState))
                {
                    if (predicate(element, currentState, additionalState))
                    {
                        state = EntityState.Modified;
                    }
                    else
                    {
                        state = EntityState.Deleted;
                    }
                }
                else
                {
                    if (predicate(element, currentState, additionalState))
                    {
                        state = EntityState.Added;
                    }
                    else
                    {
                        continue;
                    }
                }

                var serialized = serializeElement(element, state, context);
                if (serialized != null)
                {
                    serializedElements.Add(serialized);
                }
            }

            foreach (var element in removed)
            {
                if (predicate(element, previousState, additionalState))
                {
                    serializedElements.Add(serializeElement(element, EntityState.Deleted, context));
                }
            }

            return serializedElements;
        }
    }
}