﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs
{
    public class GameTransmissionProtocol
    {
        public List<object> Serialize(
            GameEntity playerEntity, EntityState state, GameSnapshot snapshot, SerializationContext context)
            => GameSnapshot.Serialize(playerEntity, state, snapshot, context);

        public static List<List<object>> Serialize<TEntity>(
            ISnapshotableCollection<TEntity> collection,
            Func<TEntity, EntityState?, SerializationContext, List<object>> serializeElement,
            SerializationContext context)
            where TEntity : Entity
            => Serialize(
                collection,
                collection.Snapshot,
                serializeElement,
                new HashSet<TEntity>(EntityEqualityComparer<TEntity>.Instance),
                context);

        public static List<List<object>> Serialize<T>(
            IEnumerable<T> collection,
            HashSet<T> snapshots,
            Func<T, EntityState?, SerializationContext, List<object>> serializeElement,
            HashSet<T> removed,
            SerializationContext context)
            where T : class
        {
            removed.Clear();
            removed.EnsureCapacity(snapshots.Count);
            foreach (var snapshot in snapshots)
            {
                removed.Add(snapshot);
            }

            snapshots.Clear();

            var serializedElements = new List<List<object>>();

            foreach (var element in collection)
            {
                snapshots.Add(element);
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

        public static List<List<object>> Serialize<TEntity, TSnapshot>(
            IEnumerable<TEntity> collection,
            Dictionary<TEntity, TSnapshot> snapshots,
            Func<TEntity, EntityState?, TSnapshot, SerializationContext, List<object>> serializeElement,
            Dictionary<TEntity, TSnapshot> removed,
            SerializationContext context)
            where TEntity : GameEntity
            where TSnapshot : class, new()
        {
            removed.Clear();
            removed.EnsureCapacity(snapshots.Count);
            foreach (var snapshot in snapshots)
            {
                removed.Add(snapshot.Key, snapshot.Value);
            }

            snapshots.Clear();

            var serializedElements = new List<List<object>>();

            foreach (var element in collection)
            {
                if (!removed.TryGetValue(element, out var snapshot))
                {
                    snapshot = new TSnapshot();
                    snapshots.Add(element, snapshot);
                    serializedElements.Add(serializeElement(element, EntityState.Added, snapshot, context));

                    continue;
                }

                snapshots.Add(element, snapshot);
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
