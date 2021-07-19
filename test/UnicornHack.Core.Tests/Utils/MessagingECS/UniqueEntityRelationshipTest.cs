using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Senses;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class UniqueEntityRelationshipTest
    {
        [Fact]
        public void Relationship_is_updated()
        {
            var manager = TestHelper.CreateGameManager();
            using (var weaponEntityReference = manager.CreateEntity())
            {
                var weaponEntity = weaponEntityReference.Referenced;
                weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
                weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

                Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship[weaponEntity.Id]);

                using (var beingEntityReference = manager.CreateEntity())
                {
                    var beingEntity = beingEntityReference.Referenced;
                    var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                    beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                    beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                    being.PrimaryNaturalWeaponId = weaponEntity.Id;

                    Assert.Equal(1, manager.BeingToPrimaryNaturalWeaponRelationship.Count);
                    Assert.True(manager.BeingToPrimaryNaturalWeaponRelationship.ContainsEntity(beingEntity.Id));
                    Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship.FindEntity(beingEntity.Id));
                    Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship.Single());
                    Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship[weaponEntity.Id]);

                    beingEntity.Being.PrimaryNaturalWeaponId = null;

                    Assert.Equal(0, manager.BeingToPrimaryNaturalWeaponRelationship.Count);
                    Assert.False(manager.BeingToPrimaryNaturalWeaponRelationship.ContainsEntity(beingEntity.Id));
                    Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship.FindEntity(beingEntity.Id));
                    Assert.Empty(manager.BeingToPrimaryNaturalWeaponRelationship);
                    Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship[weaponEntity.Id]);
                }
            }

            Assert.Equal(3, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Referenced_entity_is_kept_alive()
        {
            var manager = TestHelper.CreateGameManager();
            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                using (var weaponEntityReference = manager.CreateEntity())
                {
                    var weaponEntity = weaponEntityReference.Referenced;
                    weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
                    weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

                    being.PrimaryNaturalWeaponId = weaponEntity.Id;
                }

                Assert.Equal(1, manager.BeingToPrimaryNaturalWeaponRelationship.Count);
                Assert.Equal(1, manager.ContainedItems.Count);
            }

            Assert.Equal(1, manager.BeingToPrimaryNaturalWeaponRelationship.Count);
            Assert.Equal(1, manager.ContainedItems.Count);
            Assert.Equal(3, manager.Queue.QueuedCount);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, manager.Beings.Count);
            Assert.Equal(0, manager.BeingToPrimaryNaturalWeaponRelationship.Count);
            Assert.Equal(0, manager.ContainedItems.Count);
            Assert.Equal(0, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Throws_on_conflict()
        {
            var manager = TestHelper.CreateGameManager();
            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                using (var weaponEntityReference = manager.CreateEntity())
                {
                    var weaponEntity = weaponEntityReference.Referenced;
                    weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
                    weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

                    being.PrimaryNaturalWeaponId = weaponEntity.Id;

                    using (var conflictingBeingEntityReference = manager.CreateEntity())
                    {
                        var conflictingBeingEntity = conflictingBeingEntityReference.Referenced;
                        var conflictingBeing = conflictingBeingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                        conflictingBeingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                        conflictingBeingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                        Assert.Throws<InvalidOperationException>(() => conflictingBeing.PrimaryNaturalWeaponId = 1);
                    }
                }
            }
        }

        [Fact]
        public void Referenced_entity_not_present_throws()
        {
            var manager = TestHelper.CreateGameManager();
            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                being.PrimaryNaturalWeaponId = 1;

                Assert.Throws<InvalidOperationException>(() =>
                    beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical));
            }
        }

        [Fact]
        public void Referenced_entity_not_present_is_found_when_loaded()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            Assert.True(manager.BeingToPrimaryNaturalWeaponRelationship.IsLoading);

            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                being.PrimaryNaturalWeaponId = 2;

                using (var weaponEntityReference = manager.CreateEntity())
                {
                    var weaponEntity = weaponEntityReference.Referenced;
                    weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
                    weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

                    Assert.Equal(2, weaponEntity.Id);
                    Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship[weaponEntity.Id]);
                }
            }

            manager.IsLoading = false;

            Assert.Equal(0, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Referenced_entity_not_loaded_referencing_is_not_kept_alive()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                being.PrimaryNaturalWeaponId = 1;
            }

            Assert.Equal(0, manager.ContainedItems.Count);

            manager.IsLoading = false;

            Assert.Equal(0, manager.Queue.QueuedCount);
        }

        [Fact]
        public void Referenced_entity_not_loaded_referencing_throws_on_conflict()
        {
            var manager = TestHelper.CreateGameManager();
            manager.IsLoading = true;

            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                being.PrimaryNaturalWeaponId = 1;

                using (var conflictingBeingEntityReference = manager.CreateEntity())
                {
                    var conflictingBeingEntity = conflictingBeingEntityReference.Referenced;
                    var conflictingBeing = conflictingBeingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                    conflictingBeingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                    conflictingBeingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                    Assert.Throws<InvalidOperationException>(() => conflictingBeing.PrimaryNaturalWeaponId = 1);
                }
            }
        }

        [Fact]
        public void Queues_messages_for_registered_events()
        {
            var manager = TestHelper.CreateGameManager();

            Assert.Equal(nameof(manager.BeingToPrimaryNaturalWeaponRelationship), manager.BeingToPrimaryNaturalWeaponRelationship.Name);

            RelationshipTestSystem testSystem;
            using (var weaponEntityReference = manager.CreateEntity())
            {
                var weaponEntity = weaponEntityReference.Referenced;
                weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
                weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

                using (var beingEntityReference = ((IEntityManager)manager).CreateEntity())
                {
                    var beingEntity = (GameEntity)beingEntityReference.Referenced;
                    var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                    beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                    beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                    manager.Queue.ProcessQueue(manager);

                    testSystem = new RelationshipTestSystem(beingEntity, being, manager.BeingToPrimaryNaturalWeaponRelationship);

                    manager.Queue.Add<EntityAddedMessage<GameEntity>>(
                        testSystem, manager.BeingToPrimaryNaturalWeaponRelationship.GetEntityAddedMessageName(), 10);
                    manager.Queue.Add<EntityRemovedMessage<GameEntity>>(
                        testSystem, manager.BeingToPrimaryNaturalWeaponRelationship.GetEntityRemovedMessageName(), 10);
                    manager.Queue.Add<PropertyValueChangedMessage<GameEntity, int>>(
                        testSystem, manager.BeingToPrimaryNaturalWeaponRelationship.GetPropertyValueChangedMessageName(
                            nameof(BeingComponent.ColdResistance)), 10);
                    manager.BeingToPrimaryNaturalWeaponRelationship.AddListener(testSystem);

                    being.PrimaryNaturalWeaponId = weaponEntity.Id;

                    Assert.Equal(1, manager.Queue.QueuedCount);

                    being.ColdResistance = 10;

                    Assert.Equal(2, manager.Queue.QueuedCount);
                }
            }

            Assert.Equal(1, manager.Beings.Count);
            Assert.Equal(1, manager.ContainedItems.Count);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, manager.Beings.Count);
            Assert.Equal(0, manager.ContainedItems.Count);
            Assert.Equal(0, manager.Queue.QueuedCount);
            Assert.Equal(3, testSystem.MessagesProcessed);
            Assert.Equal(3, testSystem.GroupChangesDetected);
        }

        private class RelationshipTestSystem :
            IGameSystem<EntityAddedMessage<GameEntity>>,
            IGameSystem<EntityRemovedMessage<GameEntity>>,
            IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
            IGroupChangesListener<GameEntity>
        {
            private readonly GameEntity _testEntity;
            private readonly GameComponent _testComponent;
            private readonly IEntityGroup<GameEntity> _group;

            public RelationshipTestSystem(GameEntity testEntity, GameComponent testComponent, IEntityGroup<GameEntity> group)
            {
                _testEntity = testEntity;
                _testComponent = testComponent;
                _group = group;
            }

            public int MessagesProcessed { get; private set; }
            public int GroupChangesDetected { get; private set; }

            public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Same(_testComponent, message.ChangedComponent);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Same(_testComponent, message.ChangedComponent);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, int> message, GameManager state)
            {
                Assert.Same(_testEntity, message.Entity);
                Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, message.ChangedComponent);
                Assert.Equal(nameof(BeingComponent.ColdResistance), message.ChangedPropertyName);
                Assert.Equal(0, message.OldValue);
                Assert.Equal(10, message.NewValue);

                MessagesProcessed++;
                return MessageProcessingResult.ContinueProcessing;
            }

            public void HandleEntityAdded(
                GameEntity entity, Component addedComponent, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Same(_testComponent, addedComponent);
                Assert.Same(_group, group);

                GroupChangesDetected++;
            }

            public void HandleEntityRemoved(
                GameEntity entity, Component removedComponent, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Same(_testComponent, removedComponent);
                Assert.Same(_group, group);

                GroupChangesDetected++;
            }

            public bool HandlePropertyValueChanged<T>(
                string propertyName, T oldValue, T newValue, int componentId, Component component,
                GameEntity entity, IEntityGroup<GameEntity> group)
            {
                Assert.Same(_testEntity, entity);
                Assert.Equal(_testComponent.ComponentId, componentId);
                Assert.Same(_testComponent, component);
                Assert.Equal(nameof(BeingComponent.ColdResistance), propertyName);
                Assert.Equal(0, (int)(object)oldValue);
                Assert.Equal(10, (int)(object)newValue);
                Assert.Same(_group, group);

                GroupChangesDetected++;

                return false;
            }

            public bool HandlePropertyValuesChanged(
                IReadOnlyList<IPropertyValueChange> changes, GameEntity entity, IEntityGroup<GameEntity> group)
            {
                var change = (PropertyValueChange<int>)changes[0];

                Assert.Same(_testEntity, entity);
                Assert.Equal(_testComponent.ComponentId, change.ChangedComponent?.ComponentId);
                Assert.Same(_testComponent, change.ChangedComponent);
                Assert.Equal(nameof(BeingComponent.ColdResistance), change.ChangedPropertyName);
                Assert.Equal(0, change.OldValue);
                Assert.Equal(10, change.NewValue);
                Assert.Same(_group, group);

                GroupChangesDetected++;

                return false;
            }
        }
    }
}
