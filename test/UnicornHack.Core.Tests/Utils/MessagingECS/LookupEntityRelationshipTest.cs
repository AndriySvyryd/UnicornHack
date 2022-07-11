using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Utils.MessagingECS;

public class LookupEntityRelationshipTest
{
    [Fact]
    public void Relationship_is_updated()
    {
        var manager = TestHelper.CreateGameManager();
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        using (var firstPositionEntityReference = manager.CreateEntity())
        {
            var firstPositionEntity = firstPositionEntityReference.Referenced;
            firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            firstPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            firstPosition.LevelId = levelEntity.Id;
            firstPosition.LevelCell = new Point(2, 3);
            firstPositionEntity.Position = firstPosition;

            Assert.True(
                manager.LevelActorsToLevelCellRelationship.Dependents.ContainsEntity(firstPositionEntity.Id));
            Assert.Same(firstPositionEntity,
                manager.LevelActorsToLevelCellRelationship.Dependents.FindEntity(firstPositionEntity.Id));
            Assert.Same(firstPositionEntity,
                manager.LevelActorsToLevelCellRelationship.GetDependents(levelEntity)
                    .GetValueOrDefault(new Point(2, 3)));
            Assert.Same(firstPositionEntity, levelEntity.Level!.Actors.GetValueOrDefault(new Point(2, 3)));
            Assert.Same(levelEntity, manager.LevelActorsToLevelCellRelationship.GetPrincipal(firstPositionEntity));
            Assert.Same(levelEntity, firstPositionEntity.Position.LevelEntity);


            firstPosition.LevelCell = new Point(1, 2);

            Assert.Same(firstPositionEntity, levelEntity.Level.Actors.GetValueOrDefault(new Point(1, 2)));
            Assert.Null(levelEntity.Level.Actors.GetValueOrDefault(new Point(2, 3)));

            using (var secondPositionEntityReference = manager.CreateEntity())
            {
                var secondPositionEntity = secondPositionEntityReference.Referenced;
                secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                secondPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                secondPosition.LevelId = levelEntity.Id;
                secondPosition.LevelX = 2;
                secondPosition.LevelY = 3;
                secondPositionEntity.Position = secondPosition;

                Assert.Same(secondPositionEntity, levelEntity.Level.Actors.GetValueOrDefault(new Point(2, 3)));

                secondPositionEntity.Position = null;

                Assert.Null(levelEntity.Level.Actors.GetValueOrDefault(new Point(2, 3)));
            }

            Assert.Same(firstPositionEntity, levelEntity.Level.Actors.GetValueOrDefault(new Point(1, 2)));

            using (var secondLevelEntityReference = manager.CreateEntity())
            {
                var secondLevelEntity = secondLevelEntityReference.Referenced;
                secondLevelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);
                firstPosition.LevelId = secondLevelEntity.Id;

                Assert.Null(manager.LevelActorsToLevelCellRelationship.GetDependents(levelEntity)
                    .GetValueOrDefault(new Point(1, 2)));
                Assert.Null(levelEntity.Level.Actors.GetValueOrDefault(new Point(1, 2)));

                secondLevelEntity.Level = null;
            }

            Assert.Null(firstPositionEntity.Position);
            Assert.False(
                manager.LevelActorsToLevelCellRelationship.Dependents.ContainsEntity(firstPositionEntity.Id));
            Assert.Null(manager.LevelActorsToLevelCellRelationship.Dependents.FindEntity(firstPositionEntity.Id));
            Assert.Null(manager.LevelActorsToLevelCellRelationship.GetPrincipal(firstPositionEntity));
        }
    }

    [Fact]
    public void Throws_on_conflict()
    {
        var manager = TestHelper.CreateGameManager();
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        using var firstPositionEntityReference = manager.CreateEntity();
        var firstPositionEntity = firstPositionEntityReference.Referenced;
        firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        firstPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstPosition.LevelId = levelEntity.Id;
        firstPosition.LevelCell = new Point(2, 3);
        firstPositionEntity.Position = firstPosition;

        Assert.Same(firstPositionEntity, levelEntity.Level!.Actors[new Point(2, 3)]);

        using (var secondPositionEntityReference = manager.CreateEntity())
        {
            var secondPositionEntity = secondPositionEntityReference.Referenced;
            secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            secondPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            secondPosition.LevelId = levelEntity.Id;
            secondPosition.LevelX = 2;
            secondPosition.LevelY = 3;

            Assert.Throws<ArgumentException>(() => secondPositionEntity.Position = secondPosition);
        }
    }

    [Fact]
    public void Referenced_entity_not_present_throws()
    {
        var manager = TestHelper.CreateGameManager();
        using var raceEntityReference = manager.CreateEntity();
        var raceEntity = raceEntityReference.Referenced;
        raceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
        var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

        Assert.Throws<InvalidOperationException>(() => effect.AffectedEntityId = 2);
    }

    [Fact]
    public void Referenced_entity_not_present_is_found_when_loaded()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

        using (var raceEntityReference = manager.CreateEntity())
        {
            var raceEntity = raceEntityReference.Referenced;
            raceEntity.AddComponent<RaceComponent>((int)EntityComponent.Race);
            var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
            effect.Duration = EffectDuration.Infinite;
            effect.AffectedEntityId = 2;

            using (var beingEntityReference = manager.CreateEntity())
            {
                var beingEntity = beingEntityReference.Referenced;
                beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
                beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

                Assert.Equal(2, beingEntity.Id);
                Assert.Same(raceEntity, beingEntity.Being!.Races.Single());
            }
        }

        manager.IsLoading = false;

        Assert.Equal(1, manager.Queue.QueuedCount);
    }

    [Fact]
    public void Queues_messages_for_registered_events()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(nameof(manager.RacesToBeingRelationship), manager.RacesToBeingRelationship.Name);

        RelationshipTestSystem testSystem;
        using (var beingEntityReference = ((IEntityManager)manager).CreateEntity())
        {
            var beingEntity = (GameEntity)beingEntityReference.Referenced;
            beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
            beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

            using (var raceEntityReference = manager.CreateEntity())
            {
                var raceEntity = raceEntityReference.Referenced;
                var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
                effect.EffectType = EffectType.Move;
                effect.Duration = EffectDuration.Infinite;
                effect.AffectedEntityId = beingEntity.Id;
                var race = manager.CreateComponent<RaceComponent>((int)EntityComponent.Race);

                manager.Queue.ProcessQueue(manager);

                testSystem = new RelationshipTestSystem(raceEntity, race, manager.RacesToBeingRelationship.Dependents);

                manager.Queue.Register<EntityAddedMessage<GameEntity>>(
                    testSystem, manager.RacesToBeingRelationship.Dependents.GetEntityAddedMessageName(), 10);
                manager.Queue.Register<EntityRemovedMessage<GameEntity>>(
                    testSystem, manager.RacesToBeingRelationship.Dependents.GetEntityRemovedMessageName(), 10);
                manager.Queue.Register<PropertyValueChangedMessage<GameEntity, Species>>(
                    testSystem, manager.RacesToBeingRelationship.Dependents.GetPropertyValueChangedMessageName(
                        nameof(RaceComponent.Species)), 10);
                manager.RacesToBeingRelationship.Dependents.AddListener(testSystem);

                raceEntity.Race = race;

                Assert.Equal(2, manager.Queue.QueuedCount);

                race.Species = Species.Dragon;

                Assert.Equal(3, manager.Queue.QueuedCount);
            }
        }

        Assert.Equal(1, manager.Beings.Count);
        Assert.Equal(0, manager.ContainedItems.Count);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, manager.Queue.QueuedCount);
        Assert.Equal(0, manager.Beings.Count);
        Assert.Equal(3, testSystem.MessagesProcessed);
        Assert.Equal(3, testSystem.GroupChangesDetected);
    }

    private class RelationshipTestSystem :
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, Species>>,
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
            Assert.Equal(0, message.PropertyChanges.Count);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Null(message.RemovedComponent);
            Assert.Equal(0, message.PropertyChanges.Count);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, Species> message,
            GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
            Assert.Same(_testComponent, message.ChangedComponent);
            Assert.Equal(nameof(RaceComponent.Species), message.ChangedPropertyName);
            Assert.Equal(Species.Default, message.OldValue);
            Assert.Equal(Species.Dragon, message.NewValue);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Null(entityChange.RemovedComponent);
            Assert.Equal(0, entityChange.PropertyChanges.Count);

            GroupChangesDetected++;
        }

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
        {
            Assert.Same(_testEntity, entityChange.Entity);
            Assert.Null(entityChange.RemovedComponent);
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
            Assert.Equal(nameof(RaceComponent.Species), changes.GetChangedPropertyName(0));
            Assert.Equal(Species.Default, changes.GetValue<Species>(0, ValueType.Old));
            Assert.Equal(Species.Dragon, changes.GetValue<Species>(0, ValueType.Current));

            GroupChangesDetected++;

            return false;
        }
    }
}
