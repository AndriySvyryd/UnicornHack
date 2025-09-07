using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Utils.MessagingECS;

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

            Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship.GetDependent(weaponEntity));

            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
                being.PrimaryNaturalWeaponId = weaponEntity.Id;

                Assert.True(manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.ContainsEntity(beingEntity.Id));
                Assert.Same(beingEntity,
                    manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.FindEntity(beingEntity.Id));
                Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship.GetDependent(weaponEntity));

                beingEntity.Being!.PrimaryNaturalWeaponId = null;

                Assert.False(manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.ContainsEntity(beingEntity.Id));
                Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.FindEntity(beingEntity.Id));
                Assert.Null(manager.BeingToPrimaryNaturalWeaponRelationship.GetDependent(weaponEntity));
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

            Assert.Equal(1, manager.ContainedItems.Count);
        }

        Assert.Equal(1, manager.ContainedItems.Count);
        Assert.Equal(3, manager.Queue.QueuedCount);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, manager.Beings.Count);
        Assert.Equal(0, manager.ContainedItems.Count);
        Assert.Equal(0, manager.Queue.QueuedCount);
    }

    [Fact]
    public void Throws_on_conflict()
    {
        var manager = TestHelper.CreateGameManager();
        using var beingEntityReference = manager.CreateEntity();
        var beingEntity = beingEntityReference.Referenced;
        var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

        using var weaponEntityReference = manager.CreateEntity();
        var weaponEntity = weaponEntityReference.Referenced;
        weaponEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
        weaponEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);

        being.PrimaryNaturalWeaponId = weaponEntity.Id;

        using var conflictingBeingEntityReference = manager.CreateEntity();
        var conflictingBeingEntity = conflictingBeingEntityReference.Referenced;
        var conflictingBeing =
            conflictingBeingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        conflictingBeingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        conflictingBeingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

        Assert.Throws<InvalidOperationException>(() => conflictingBeing.PrimaryNaturalWeaponId = 1);
    }

    [Fact]
    public void Referenced_entity_not_present_throws()
    {
        var manager = TestHelper.CreateGameManager();
        using var beingEntityReference = manager.CreateEntity();
        var beingEntity = beingEntityReference.Referenced;
        var being = beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);
        being.PrimaryNaturalWeaponId = 1;

        Assert.Throws<InvalidOperationException>(() =>
            beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical));
    }

    [Fact]
    public void Referenced_entity_not_present_is_found_when_loaded()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

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
                Assert.Same(beingEntity, manager.BeingToPrimaryNaturalWeaponRelationship.GetDependent(weaponEntity));
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

        Assert.Equal(nameof(manager.BeingToPrimaryNaturalWeaponRelationship),
            manager.BeingToPrimaryNaturalWeaponRelationship.Name);

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

                testSystem = new RelationshipTestSystem(beingEntity, being,
                    manager.BeingToPrimaryNaturalWeaponRelationship.Dependents);

                manager.Queue.Register<EntityAddedMessage<GameEntity>>(
                    testSystem, manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.GetEntityAddedMessageName(),
                    10);
                manager.Queue.Register<EntityRemovedMessage<GameEntity>>(
                    testSystem,
                    manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.GetEntityRemovedMessageName(), 10);
                manager.Queue.Register<PropertyValueChangedMessage<GameEntity, int>>(
                    testSystem, manager.BeingToPrimaryNaturalWeaponRelationship.Dependents
                        .GetPropertyValueChangedMessageName(
                            nameof(BeingComponent.ColdResistance)), 10);
                manager.BeingToPrimaryNaturalWeaponRelationship.Dependents.AddListener(testSystem);

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
        IEntityChangeListener<GameEntity>
    {
        private readonly GameEntity _testEntity;
        private readonly GameComponent _testComponent;
        private readonly IEntityGroup<GameEntity> _group;

        public RelationshipTestSystem(GameEntity testEntity, GameComponent testComponent,
            IEntityGroup<GameEntity> group)
        {
            _testEntity = testEntity;
            _testComponent = testComponent;
            _group = group;
        }

        public int MessagesProcessed
        {
            get;
            private set;
        }

        public int GroupChangesDetected
        {
            get;
            private set;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Null(message.RemovedComponent);
            Assert.Equal(1, message.PropertyChanges.Count);
            Assert.Same(_testComponent, message.PropertyChanges.GetChangedComponent(0));

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Same(_testComponent, message.RemovedComponent);
            Assert.Equal(0, message.PropertyChanges.Count);

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

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Null(entityChange.RemovedComponent);
            Assert.Equal(1, entityChange.PropertyChanges.Count);
            Assert.Same(_testComponent, entityChange.PropertyChanges.GetChangedComponent(0));

            GroupChangesDetected++;
        }

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Same(_testComponent, entityChange.RemovedComponent);
            Assert.Equal(0, entityChange.PropertyChanges.Count);

            GroupChangesDetected++;
        }

        public bool OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        {
            var changes = entityChange.PropertyChanges;

            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Equal(1, entityChange.PropertyChanges.Count);
            Assert.Equal(_testComponent.ComponentId, changes.GetChangedComponent(0).ComponentId);
            Assert.Same(_testComponent, changes.GetChangedComponent(0));
            Assert.Equal(nameof(BeingComponent.ColdResistance), changes.GetChangedPropertyName(0));
            Assert.Equal(0, changes.GetValue<int>(0, ValueType.Old));
            Assert.Equal(10, changes.GetValue<int>(0, ValueType.Current));

            GroupChangesDetected++;

            return false;
        }
    }
}
