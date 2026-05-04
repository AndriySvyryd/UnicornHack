using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
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

    [Fact]
    public void Dependent_listener_receives_property_changes()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new DependentTestListener();
        manager.RacesToBeingRelationship.AddDependentsListener(listener);

        using var beingEntityReference = manager.CreateEntity();
        var beingEntity = beingEntityReference.Referenced;
        beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

        using var raceEntityReference = manager.CreateEntity();
        var raceEntity = raceEntityReference.Referenced;
        var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
        effect.EffectType = EffectType.Move;
        effect.Duration = EffectDuration.Infinite;
        effect.AffectedEntityId = beingEntity.Id;
        var race = manager.CreateComponent<RaceComponent>((int)EntityComponent.Race);
        raceEntity.Race = race;

        Assert.Equal(1, listener.AddedCount);
        Assert.Equal(0, listener.PropertyChangedCount);

        race.Species = Species.Dragon;

        Assert.Equal(1, listener.PropertyChangedCount);
        Assert.Same(raceEntity, listener.LastPropertyChangedEntity);
        Assert.Same(beingEntity, listener.LastPrincipal);
    }

    [Fact]
    public void Dependent_listener_does_not_receive_property_changes_on_removal()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new DependentTestListener();
        manager.RacesToBeingRelationship.AddDependentsListener(listener);

        using var beingEntityReference = manager.CreateEntity();
        var beingEntity = beingEntityReference.Referenced;
        beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        beingEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        beingEntity.AddComponent<SensorComponent>((int)EntityComponent.Sensor);

        using var raceEntityReference = manager.CreateEntity();
        var raceEntity = raceEntityReference.Referenced;
        var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
        effect.EffectType = EffectType.Move;
        effect.Duration = EffectDuration.Infinite;
        effect.AffectedEntityId = beingEntity.Id;
        var race = manager.CreateComponent<RaceComponent>((int)EntityComponent.Race);
        raceEntity.Race = race;

        Assert.Equal(1, listener.AddedCount);

        // Removing the Race component removes the entity from the Races group,
        // which should not fire a property change on the dependent listener.
        raceEntity.Race = null;

        Assert.Equal(0, listener.PropertyChangedCount);
        Assert.Equal(1, listener.RemovedCount);
    }

    /// <summary>
    ///     Regression: a single dependent group (e.g. <c>KnownPositions</c>) is shared by
    ///     multiple lookup relationships that differ only by their secondary principal
    ///     (<c>LevelActors</c> vs <c>LevelItems</c>). When a dependent leaves the shared
    ///     group, every relationship's <c>TryRemoveEntity</c> runs — including for
    ///     relationships that never tracked the entity. The lookup must verify that the
    ///     dictionary slot at the dependent's secondary key actually holds the same
    ///     entity before removing it; otherwise an unrelated entity occupying that slot
    ///     gets silently evicted and a bogus <c>OnEntityRemoved</c> fires for a foreign
    ///     entity.
    /// </summary>
    [Fact]
    public void Removing_dependent_does_not_evict_unrelated_entity_at_same_secondary_key()
    {
        var manager = TestHelper.CreateGameManager();

        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        var sharedCell = new Point(5, 6);

        // Underlying actor (in LevelActors group, the secondary principal for KnownActors)
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.LevelId = levelEntity.Id;
        actorPosition.LevelX = 10;
        actorPosition.LevelY = 11;
        actorEntity.Position = actorPosition;

        // Underlying item (in LevelItems group, the secondary principal for KnownItems)
        using var itemEntityReference = manager.CreateEntity();
        var itemEntity = itemEntityReference.Referenced;
        itemEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
        itemEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var itemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemPosition.LevelId = levelEntity.Id;
        itemPosition.LevelX = 12;
        itemPosition.LevelY = 13;
        itemEntity.Position = itemPosition;

        // Actor-knowledge entity at sharedCell — populates level.KnownActors[sharedCell]
        using var actorKnowledgeReference = manager.CreateEntity();
        var actorKnowledgeEntity = actorKnowledgeReference.Referenced;
        var actorKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        actorKnowledge.KnownEntityId = actorEntity.Id;
        actorKnowledgeEntity.Knowledge = actorKnowledge;
        var actorKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorKnowledgePosition.LevelId = levelEntity.Id;
        actorKnowledgePosition.LevelX = (byte)sharedCell.X;
        actorKnowledgePosition.LevelY = (byte)sharedCell.Y;
        actorKnowledgeEntity.Position = actorKnowledgePosition;

        // Item-knowledge entity at the SAME cell — populates level.KnownItems[sharedCell]
        using var itemKnowledgeReference = manager.CreateEntity();
        var itemKnowledgeEntity = itemKnowledgeReference.Referenced;
        var itemKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        itemKnowledge.KnownEntityId = itemEntity.Id;
        itemKnowledgeEntity.Knowledge = itemKnowledge;
        var itemKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemKnowledgePosition.LevelId = levelEntity.Id;
        itemKnowledgePosition.LevelX = (byte)sharedCell.X;
        itemKnowledgePosition.LevelY = (byte)sharedCell.Y;
        itemKnowledgeEntity.Position = itemKnowledgePosition;

        Assert.Same(actorKnowledgeEntity, levelEntity.Level!.KnownActors[sharedCell]);
        Assert.Same(itemKnowledgeEntity, levelEntity.Level.KnownItems[sharedCell]);

        // Track removal events on the actor relationship to ensure no spurious removal
        // fires for the item-knowledge entity (which it never tracked).
        var actorListener = new DependentTestListener();
        manager.KnownActorsToLevelCellRelationship.AddDependentsListener(actorListener);

        // Remove the item-knowledge from the shared dependent group. This triggers
        // KnownActorsToLevelCellRelationship.TryRemoveEntity for the item-knowledge
        // even though it was never tracked there.
        itemKnowledgeEntity.Knowledge = null;

        // The actor relationship must be untouched — actor-knowledge still in place.
        Assert.Same(actorKnowledgeEntity, levelEntity.Level.KnownActors[sharedCell]);
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(actorKnowledgeEntity.Id));
        Assert.Equal(0, actorListener.RemovedCount);

        // The item-knowledge is gone from the items lookup as expected.
        Assert.False(levelEntity.Level.KnownItems.ContainsKey(sharedCell));
    }

    /// <summary>
    ///     Regression: similar to the removal case, but triggered through
    ///     <c>HandleNonKeyPropertyValuesChanged</c>. When a dependent's secondary-principal
    ///     key changes to a value that's not in the secondary principal group, the
    ///     relationship runs <c>entities.Remove(oldSortKey)</c>. If the dependent was
    ///     never tracked by this relationship in the first place (it lived in a sibling
    ///     relationship sharing the same dependent group), the slot at <c>oldSortKey</c>
    ///     may legitimately hold a different entity, which must not be evicted.
    /// </summary>
    [Fact]
    public void Secondary_principal_key_change_does_not_evict_unrelated_entity_at_same_secondary_key()
    {
        var manager = TestHelper.CreateGameManager();

        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

        var sharedCell = new Point(5, 6);

        // Underlying actor (in LevelActors group, secondary principal for KnownActors)
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.LevelId = levelEntity.Id;
        actorPosition.LevelX = 10;
        actorPosition.LevelY = 11;
        actorEntity.Position = actorPosition;

        // Two items (in LevelItems group, secondary principal for KnownItems)
        using var firstItemReference = manager.CreateEntity();
        var firstItem = firstItemReference.Referenced;
        firstItem.AddComponent<ItemComponent>((int)EntityComponent.Item);
        firstItem.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var firstItemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstItemPosition.LevelId = levelEntity.Id;
        firstItemPosition.LevelX = 12;
        firstItemPosition.LevelY = 13;
        firstItem.Position = firstItemPosition;

        using var secondItemReference = manager.CreateEntity();
        var secondItem = secondItemReference.Referenced;
        secondItem.AddComponent<ItemComponent>((int)EntityComponent.Item);
        secondItem.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var secondItemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        secondItemPosition.LevelId = levelEntity.Id;
        secondItemPosition.LevelX = 14;
        secondItemPosition.LevelY = 15;
        secondItem.Position = secondItemPosition;

        // Actor-knowledge entity at sharedCell — populates level.KnownActors[sharedCell]
        using var actorKnowledgeReference = manager.CreateEntity();
        var actorKnowledgeEntity = actorKnowledgeReference.Referenced;
        var actorKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        actorKnowledge.KnownEntityId = actorEntity.Id;
        actorKnowledgeEntity.Knowledge = actorKnowledge;
        var actorKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorKnowledgePosition.LevelId = levelEntity.Id;
        actorKnowledgePosition.LevelX = (byte)sharedCell.X;
        actorKnowledgePosition.LevelY = (byte)sharedCell.Y;
        actorKnowledgeEntity.Position = actorKnowledgePosition;

        // Item-knowledge entity at the SAME cell, initially tracking firstItem
        using var itemKnowledgeReference = manager.CreateEntity();
        var itemKnowledgeEntity = itemKnowledgeReference.Referenced;
        var itemKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        itemKnowledge.KnownEntityId = firstItem.Id;
        itemKnowledgeEntity.Knowledge = itemKnowledge;
        var itemKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemKnowledgePosition.LevelId = levelEntity.Id;
        itemKnowledgePosition.LevelX = (byte)sharedCell.X;
        itemKnowledgePosition.LevelY = (byte)sharedCell.Y;
        itemKnowledgeEntity.Position = itemKnowledgePosition;

        Assert.Same(actorKnowledgeEntity, levelEntity.Level!.KnownActors[sharedCell]);
        Assert.Same(itemKnowledgeEntity, levelEntity.Level.KnownItems[sharedCell]);

        // Track removals on the actor relationship — none should fire for the item-knowledge.
        var actorListener = new DependentTestListener();
        manager.KnownActorsToLevelCellRelationship.AddDependentsListener(actorListener);

        // Mutate the item-knowledge's secondary-principal key (KnownEntityId) from one
        // item to another. Both target ids are absent from LevelActors, so the actor
        // relationship's HandleNonKeyPropertyValuesChanged takes the secondary-principal-
        // key-changed → "new principal not in secondary group" branch and would call
        // entities.Remove(sharedCell) on level.KnownActors without an identity check.
        itemKnowledge.KnownEntityId = secondItem.Id;

        // The actor lookup must be untouched.
        Assert.Same(actorKnowledgeEntity, levelEntity.Level.KnownActors[sharedCell]);
        Assert.True(manager.KnownActorsToLevelCellRelationship.Dependents.ContainsEntity(actorKnowledgeEntity.Id));
        Assert.Equal(0, actorListener.RemovedCount);

        // The item-knowledge should now be tracking secondItem under KnownItems.
        Assert.Same(itemKnowledgeEntity, levelEntity.Level.KnownItems[sharedCell]);
    }

    private class DependentTestListener : IDependentEntityChangeListener<GameEntity>
    {
        public int AddedCount
        {
            get; private set;
        }
        public int RemovedCount
        {
            get; private set;
        }
        public int PropertyChangedCount
        {
            get; private set;
        }
        public GameEntity? LastPropertyChangedEntity
        {
            get; private set;
        }
        public GameEntity? LastPrincipal
        {
            get; private set;
        }

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange, GameEntity principal)
            => AddedCount++;

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange, GameEntity principal)
            => RemovedCount++;

        public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange, GameEntity principal)
        {
            PropertyChangedCount++;
            LastPropertyChangedEntity = entityChange.Entity;
            LastPrincipal = principal;
        }
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

        public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
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
        }
    }
}
