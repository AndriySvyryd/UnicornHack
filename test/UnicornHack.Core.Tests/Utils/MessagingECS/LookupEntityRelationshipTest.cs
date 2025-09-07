using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Knowledge;
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

    [Fact]
    public void Secondary_principal_removal_causes_dependent_to_be_removed()
    {
        var manager = TestHelper.CreateGameManager();

        // Create a level
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        // Create an actor first that will be referenced
        var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.LevelId = levelEntity.Id;
        actorPosition.LevelX = 10;
        actorPosition.LevelY = 11;
        actorEntity.Position = actorPosition;

        // Create a knowledge entity that references the actor
        var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = actorEntity.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.LevelId = levelEntity.Id;
        position.LevelX = 5;
        position.LevelY = 6;
        knowledgeEntity.Position = position;

        actorEntityReference.Dispose();
        knowledgeEntityReference.Dispose();
        Assert.NotNull(knowledgeEntity.Manager);
        Assert.NotNull(knowledgeEntity.Manager);

        // Verify the relationship is established since both principal and secondary principal exist
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Level!.KnownActors);
        Assert.Same(knowledgeEntity, levelEntity.Level.KnownActors[new Point(5, 6)]);

        // Remove the actor from the LevelActors group (secondary principal group)
        actorEntity.AI = null;

        // The knowledge entity should be removed from the relationship and deleted
        Assert.False(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.Empty(levelEntity.Level.KnownActors);
        Assert.Null(knowledgeEntity.Manager);
    }

    [Fact]
    public void Secondary_principal_key_can_be_set_after()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

        // Create a level
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        // Create a knowledge entity that references an actor that will be created later
        using var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.LevelId = levelEntity.Id;
        position.LevelX = 5;
        position.LevelY = 6;
        knowledgeEntity.Position = position;

        // During loading, the relationship should not be established yet
        Assert.False(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));

        // Create the referenced actor during loading
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.LevelId = levelEntity.Id;
        actorPosition.LevelX = 10;
        actorPosition.LevelY = 11;
        actorEntity.Position = actorPosition;
        knowledge.KnownEntityId = actorEntity.Id;

        // The relationship should be established now that the secondary principal exists
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.False(manager.KnownItemsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownItemsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.False(manager.KnownConnectionsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownConnectionsToLevelCellRelationship.GetDependents(levelEntity));
    }

    [Fact]
    public void Secondary_principal_can_be_added_after_dependent()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        // Create an actor that will be referenced by multiple knowledge entities
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.LevelId = levelEntity.Id;
        actorPosition.LevelX = 50;
        actorPosition.LevelY = 51;

        using var knowledgeEntityRef = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityRef.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = actorEntity.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.LevelId = levelEntity.Id;
        position.LevelX = 1;
        position.LevelY = 2;
        knowledgeEntity.Position = position;

        // Should not be in the relationship yet because the actor doesn't have a position (not in LevelActors)
        Assert.False(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));

        // Add the actor to the LevelActors group by giving it a position
        actorEntity.Position = actorPosition;

        // The knowledge entity should now be in the relationship
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Level!.KnownActors);
    }

    [Fact]
    public void Secondary_principal_key_change_updates_relationship()
    {
        var manager = TestHelper.CreateGameManager();

        // Create a level
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        // Create two actors
        using var firstActorReference = manager.CreateEntity();
        var firstActor = firstActorReference.Referenced;
        firstActor.AddComponent<AIComponent>((int)EntityComponent.AI);
        firstActor.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var firstActorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstActorPosition.LevelId = levelEntity.Id;
        firstActorPosition.LevelX = 10;
        firstActorPosition.LevelY = 11;
        firstActor.Position = firstActorPosition;

        using var secondActorReference = manager.CreateEntity();
        var secondActor = secondActorReference.Referenced;
        secondActor.AddComponent<AIComponent>((int)EntityComponent.AI);
        secondActor.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var secondActorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        secondActorPosition.LevelId = levelEntity.Id;
        secondActorPosition.LevelX = 20;
        secondActorPosition.LevelY = 21;
        secondActor.Position = secondActorPosition;

        // Create a knowledge entity that references the first actor
        using var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = firstActor.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.LevelId = levelEntity.Id;
        position.LevelX = 5;
        position.LevelY = 6;
        knowledgeEntity.Position = position;

        // Verify the relationship is established
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));

        // Change the knowledge entity to reference the second actor
        knowledge.KnownEntityId = secondActor.Id;

        // The relationship should still be maintained (different secondary principal reference)
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToLevelCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Level!.KnownActors);
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
