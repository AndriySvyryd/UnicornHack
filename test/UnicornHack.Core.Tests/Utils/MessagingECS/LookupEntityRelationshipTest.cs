using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class LookupEntityRelationshipTest
{
    [Fact]
    public void Relationship_is_updated()
    {
        var manager = TestHelper.CreateGameManager();
        using var levelEntityReference = manager.CreateEntity();
        var roomEntity = levelEntityReference.Referenced;
        roomEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        using (var firstPositionEntityReference = manager.CreateEntity())
        {
            var firstPositionEntity = firstPositionEntityReference.Referenced;
            firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            firstPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            firstPosition.RoomId = roomEntity.Id;
            firstPosition.Cell = new Point(2, 3);
            firstPositionEntity.Position = firstPosition;

            Assert.True(
                manager.RoomActorsToRoomCellRelationship.Dependents.ContainsEntity(firstPositionEntity.Id));
            Assert.Same(firstPositionEntity,
                manager.RoomActorsToRoomCellRelationship.Dependents.FindEntity(firstPositionEntity.Id));
            Assert.Same(firstPositionEntity,
                manager.RoomActorsToRoomCellRelationship.GetDependents(roomEntity)
                    .GetValueOrDefault(new Point(2, 3)));
            Assert.Same(firstPositionEntity, roomEntity.Room!.Actors.GetValueOrDefault(new Point(2, 3)));
            Assert.Same(roomEntity, manager.RoomActorsToRoomCellRelationship.GetPrincipal(firstPositionEntity));
            Assert.Same(roomEntity, firstPositionEntity.Position.RoomEntity);


            firstPosition.Cell = new Point(1, 2);

            Assert.Same(firstPositionEntity, roomEntity.Room.Actors.GetValueOrDefault(new Point(1, 2)));
            Assert.Null(roomEntity.Room.Actors.GetValueOrDefault(new Point(2, 3)));

            using (var secondPositionEntityReference = manager.CreateEntity())
            {
                var secondPositionEntity = secondPositionEntityReference.Referenced;
                secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                secondPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
                var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                secondPosition.RoomId = roomEntity.Id;
                secondPosition.CellX = 2;
                secondPosition.CellY = 3;
                secondPositionEntity.Position = secondPosition;

                Assert.Same(secondPositionEntity, roomEntity.Room.Actors.GetValueOrDefault(new Point(2, 3)));

                secondPositionEntity.Position = null;

                Assert.Null(roomEntity.Room.Actors.GetValueOrDefault(new Point(2, 3)));
            }

            Assert.Same(firstPositionEntity, roomEntity.Room.Actors.GetValueOrDefault(new Point(1, 2)));

            using (var secondLevelEntityReference = manager.CreateEntity())
            {
                var secondLevelEntity = secondLevelEntityReference.Referenced;
                secondLevelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);
                firstPosition.RoomId = secondLevelEntity.Id;

                Assert.Null(manager.RoomActorsToRoomCellRelationship.GetDependents(roomEntity)
                    .GetValueOrDefault(new Point(1, 2)));
                Assert.Null(roomEntity.Room.Actors.GetValueOrDefault(new Point(1, 2)));

                secondLevelEntity.Room = null;
            }

            Assert.Null(firstPositionEntity.Position);
            Assert.False(
                manager.RoomActorsToRoomCellRelationship.Dependents.ContainsEntity(firstPositionEntity.Id));
            Assert.Null(manager.RoomActorsToRoomCellRelationship.Dependents.FindEntity(firstPositionEntity.Id));
            Assert.Null(manager.RoomActorsToRoomCellRelationship.GetPrincipal(firstPositionEntity));
        }
    }

    [Fact]
    public void Throws_on_conflict()
    {
        var manager = TestHelper.CreateGameManager();
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        using var firstPositionEntityReference = manager.CreateEntity();
        var firstPositionEntity = firstPositionEntityReference.Referenced;
        firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        firstPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstPosition.RoomId = levelEntity.Id;
        firstPosition.Cell = new Point(2, 3);
        firstPositionEntity.Position = firstPosition;

        Assert.Same(firstPositionEntity, levelEntity.Room!.Actors[new Point(2, 3)]);

        using (var secondPositionEntityReference = manager.CreateEntity())
        {
            var secondPositionEntity = secondPositionEntityReference.Referenced;
            secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            secondPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            secondPosition.RoomId = levelEntity.Id;
            secondPosition.CellX = 2;
            secondPosition.CellY = 3;

            Assert.Throws<ArgumentException>(() => secondPositionEntity.Position = secondPosition);
        }
    }

    [Fact]
    public void Referenced_entity_not_present_throws()
    {
        var manager = TestHelper.CreateGameManager();
        using var raceEntityReference = manager.CreateEntity();
        var raceEntity = raceEntityReference.Referenced;
        raceEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var effect = raceEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

        Assert.Throws<InvalidOperationException>(() => effect.ContainingAbilityId = 2);
    }

    [Fact]
    public void Secondary_principal_removal_causes_dependent_to_be_removed()
    {
        var manager = TestHelper.CreateGameManager();

        // Create a level
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        // Create an actor first that will be referenced
        var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.RoomId = levelEntity.Id;
        actorPosition.CellX = 10;
        actorPosition.CellY = 11;
        actorEntity.Position = actorPosition;

        // Create a knowledge entity that references the actor
        var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = actorEntity.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = levelEntity.Id;
        position.CellX = 5;
        position.CellY = 6;
        knowledgeEntity.Position = position;

        actorEntityReference.Dispose();
        knowledgeEntityReference.Dispose();
        Assert.NotNull(knowledgeEntity.Manager);
        Assert.NotNull(knowledgeEntity.Manager);

        // Verify the relationship is established since both principal and secondary principal exist
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Room!.KnownActors);
        Assert.Same(knowledgeEntity, levelEntity.Room.KnownActors[new Point(5, 6)]);

        // Remove the actor from the LevelActors group (secondary principal group)
        actorEntity.AI = null;

        // The knowledge entity should be removed from the relationship and deleted
        Assert.False(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));
        Assert.Empty(levelEntity.Room.KnownActors);
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
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        // Create a knowledge entity that references an actor that will be created later
        using var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = levelEntity.Id;
        position.CellX = 5;
        position.CellY = 6;
        knowledgeEntity.Position = position;

        // During loading, the relationship should not be established yet
        Assert.False(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));

        // Create the referenced actor during loading
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.RoomId = levelEntity.Id;
        actorPosition.CellX = 10;
        actorPosition.CellY = 11;
        actorEntity.Position = actorPosition;
        knowledge.KnownEntityId = actorEntity.Id;

        // The relationship should be established now that the secondary principal exists
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));
        Assert.False(manager.KnownItemsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownItemsToCellRelationship.GetDependents(levelEntity));
        Assert.False(manager.KnownConnectionsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownConnectionsToCellRelationship.GetDependents(levelEntity));
    }

    [Fact]
    public void Secondary_principal_can_be_added_after_dependent()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        // Create an actor that will be referenced by multiple knowledge entities
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.RoomId = levelEntity.Id;
        actorPosition.CellX = 50;
        actorPosition.CellY = 51;

        using var knowledgeEntityRef = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityRef.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = actorEntity.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = levelEntity.Id;
        position.CellX = 1;
        position.CellY = 2;
        knowledgeEntity.Position = position;

        // Should not be in the relationship yet because the actor doesn't have a position (not in LevelActors)
        Assert.False(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Empty(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));

        // Add the actor to the LevelActors group by giving it a position
        actorEntity.Position = actorPosition;

        // The knowledge entity should now be in the relationship
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Room!.KnownActors);
    }

    [Fact]
    public void Secondary_principal_key_change_updates_relationship()
    {
        var manager = TestHelper.CreateGameManager();

        // Create a level
        using var levelEntityReference = manager.CreateEntity();
        var levelEntity = levelEntityReference.Referenced;
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        // Create two actors
        using var firstActorReference = manager.CreateEntity();
        var firstActor = firstActorReference.Referenced;
        firstActor.AddComponent<AIComponent>((int)EntityComponent.AI);
        firstActor.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var firstActorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstActorPosition.RoomId = levelEntity.Id;
        firstActorPosition.CellX = 10;
        firstActorPosition.CellY = 11;
        firstActor.Position = firstActorPosition;

        using var secondActorReference = manager.CreateEntity();
        var secondActor = secondActorReference.Referenced;
        secondActor.AddComponent<AIComponent>((int)EntityComponent.AI);
        secondActor.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var secondActorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        secondActorPosition.RoomId = levelEntity.Id;
        secondActorPosition.CellX = 20;
        secondActorPosition.CellY = 21;
        secondActor.Position = secondActorPosition;

        // Create a knowledge entity that references the first actor
        using var knowledgeEntityReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeEntityReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        knowledge.KnownEntityId = firstActor.Id;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = levelEntity.Id;
        position.CellX = 5;
        position.CellY = 6;
        knowledgeEntity.Position = position;

        // Verify the relationship is established
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));

        // Change the knowledge entity to reference the second actor
        knowledge.KnownEntityId = secondActor.Id;

        // The relationship should still be maintained (different secondary principal reference)
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(knowledgeEntity.Id));
        Assert.Single(manager.KnownActorsToCellRelationship.GetDependents(levelEntity));
        Assert.Single(levelEntity.Room!.KnownActors);
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
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        var sharedCell = new Point(5, 6);

        // Underlying actor (in LevelActors group, the secondary principal for KnownActors)
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.RoomId = levelEntity.Id;
        actorPosition.CellX = 10;
        actorPosition.CellY = 11;
        actorEntity.Position = actorPosition;

        // Underlying item (in LevelItems group, the secondary principal for KnownItems)
        using var itemEntityReference = manager.CreateEntity();
        var itemEntity = itemEntityReference.Referenced;
        itemEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
        itemEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var itemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemPosition.RoomId = levelEntity.Id;
        itemPosition.CellX = 12;
        itemPosition.CellY = 13;
        itemEntity.Position = itemPosition;

        // Actor-knowledge entity at sharedCell — populates level.KnownActors[sharedCell]
        using var actorKnowledgeReference = manager.CreateEntity();
        var actorKnowledgeEntity = actorKnowledgeReference.Referenced;
        var actorKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        actorKnowledge.KnownEntityId = actorEntity.Id;
        actorKnowledgeEntity.Knowledge = actorKnowledge;
        var actorKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorKnowledgePosition.RoomId = levelEntity.Id;
        actorKnowledgePosition.CellX = (byte)sharedCell.X;
        actorKnowledgePosition.CellY = (byte)sharedCell.Y;
        actorKnowledgeEntity.Position = actorKnowledgePosition;

        // Item-knowledge entity at the SAME cell — populates level.KnownItems[sharedCell]
        using var itemKnowledgeReference = manager.CreateEntity();
        var itemKnowledgeEntity = itemKnowledgeReference.Referenced;
        var itemKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        itemKnowledge.KnownEntityId = itemEntity.Id;
        itemKnowledgeEntity.Knowledge = itemKnowledge;
        var itemKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemKnowledgePosition.RoomId = levelEntity.Id;
        itemKnowledgePosition.CellX = (byte)sharedCell.X;
        itemKnowledgePosition.CellY = (byte)sharedCell.Y;
        itemKnowledgeEntity.Position = itemKnowledgePosition;

        Assert.Same(actorKnowledgeEntity, levelEntity.Room!.KnownActors[sharedCell]);
        Assert.Same(itemKnowledgeEntity, levelEntity.Room.KnownItems[sharedCell]);

        // Track removal events on the actor relationship to ensure no spurious removal
        // fires for the item-knowledge entity (which it never tracked).
        var actorListener = new DependentTestListener();
        manager.KnownActorsToCellRelationship.AddDependentsListener(actorListener);

        // Remove the item-knowledge from the shared dependent group. This triggers
        // KnownActorsToLevelCellRelationship.TryRemoveEntity for the item-knowledge
        // even though it was never tracked there.
        itemKnowledgeEntity.Knowledge = null;

        // The actor relationship must be untouched — actor-knowledge still in place.
        Assert.Same(actorKnowledgeEntity, levelEntity.Room.KnownActors[sharedCell]);
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(actorKnowledgeEntity.Id));
        Assert.Equal(0, actorListener.RemovedCount);

        // The item-knowledge is gone from the items lookup as expected.
        Assert.False(levelEntity.Room.KnownItems.ContainsKey(sharedCell));
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
        levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        var sharedCell = new Point(5, 6);

        // Underlying actor (in LevelActors group, secondary principal for KnownActors)
        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var actorPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorPosition.RoomId = levelEntity.Id;
        actorPosition.CellX = 10;
        actorPosition.CellY = 11;
        actorEntity.Position = actorPosition;

        // Two items (in LevelItems group, secondary principal for KnownItems)
        using var firstItemReference = manager.CreateEntity();
        var firstItem = firstItemReference.Referenced;
        firstItem.AddComponent<ItemComponent>((int)EntityComponent.Item);
        firstItem.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var firstItemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        firstItemPosition.RoomId = levelEntity.Id;
        firstItemPosition.CellX = 12;
        firstItemPosition.CellY = 13;
        firstItem.Position = firstItemPosition;

        using var secondItemReference = manager.CreateEntity();
        var secondItem = secondItemReference.Referenced;
        secondItem.AddComponent<ItemComponent>((int)EntityComponent.Item);
        secondItem.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
        var secondItemPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        secondItemPosition.RoomId = levelEntity.Id;
        secondItemPosition.CellX = 14;
        secondItemPosition.CellY = 15;
        secondItem.Position = secondItemPosition;

        // Actor-knowledge entity at sharedCell — populates level.KnownActors[sharedCell]
        using var actorKnowledgeReference = manager.CreateEntity();
        var actorKnowledgeEntity = actorKnowledgeReference.Referenced;
        var actorKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        actorKnowledge.KnownEntityId = actorEntity.Id;
        actorKnowledgeEntity.Knowledge = actorKnowledge;
        var actorKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        actorKnowledgePosition.RoomId = levelEntity.Id;
        actorKnowledgePosition.CellX = (byte)sharedCell.X;
        actorKnowledgePosition.CellY = (byte)sharedCell.Y;
        actorKnowledgeEntity.Position = actorKnowledgePosition;

        // Item-knowledge entity at the SAME cell, initially tracking firstItem
        using var itemKnowledgeReference = manager.CreateEntity();
        var itemKnowledgeEntity = itemKnowledgeReference.Referenced;
        var itemKnowledge = manager.CreateComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
        itemKnowledge.KnownEntityId = firstItem.Id;
        itemKnowledgeEntity.Knowledge = itemKnowledge;
        var itemKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        itemKnowledgePosition.RoomId = levelEntity.Id;
        itemKnowledgePosition.CellX = (byte)sharedCell.X;
        itemKnowledgePosition.CellY = (byte)sharedCell.Y;
        itemKnowledgeEntity.Position = itemKnowledgePosition;

        Assert.Same(actorKnowledgeEntity, levelEntity.Room!.KnownActors[sharedCell]);
        Assert.Same(itemKnowledgeEntity, levelEntity.Room.KnownItems[sharedCell]);

        // Track removals on the actor relationship — none should fire for the item-knowledge.
        var actorListener = new DependentTestListener();
        manager.KnownActorsToCellRelationship.AddDependentsListener(actorListener);

        // Mutate the item-knowledge's secondary-principal key (KnownEntityId) from one
        // item to another. Both target ids are absent from LevelActors, so the actor
        // relationship's HandleNonKeyPropertyValuesChanged takes the secondary-principal-
        // key-changed → "new principal not in secondary group" branch and would call
        // entities.Remove(sharedCell) on level.KnownActors without an identity check.
        itemKnowledge.KnownEntityId = secondItem.Id;

        // The actor lookup must be untouched.
        Assert.Same(actorKnowledgeEntity, levelEntity.Room.KnownActors[sharedCell]);
        Assert.True(manager.KnownActorsToCellRelationship.Dependents.ContainsEntity(actorKnowledgeEntity.Id));
        Assert.Equal(0, actorListener.RemovedCount);

        // The item-knowledge should now be tracking secondItem under KnownItems.
        Assert.Same(itemKnowledgeEntity, levelEntity.Room.KnownItems[sharedCell]);
    }

    [Fact]
    public void Referenced_entity_not_present_is_found_when_loaded()
    {
        var manager = TestHelper.CreateGameManager();
        manager.IsLoading = true;

        using (var actorEntityReference = manager.CreateEntity())
        {
            var actorEntity = actorEntityReference.Referenced;
            actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            position.RoomId = 2;
            position.Cell = new Point(2, 3);
            actorEntity.Position = position;

            // During loading the actor can't be linked yet: the room it references doesn't exist.
            Assert.False(manager.RoomActorsToRoomCellRelationship.Dependents.ContainsEntity(actorEntity.Id));

            using (var roomEntityReference = manager.CreateEntity())
            {
                var roomEntity = roomEntityReference.Referenced;
                roomEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

                Assert.Equal(2, roomEntity.Id);

                // Once its principal exists the deferred dependent is linked into the room's cell.
                Assert.Same(actorEntity, roomEntity.Room!.Actors.GetValueOrDefault(new Point(2, 3)));
            }
        }

        manager.IsLoading = false;
    }

    [Fact]
    public void Queues_messages_for_registered_events()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(nameof(manager.RoomActorsToRoomCellRelationship),
            manager.RoomActorsToRoomCellRelationship.Name);

        using var roomEntityReference = manager.CreateEntity();
        var roomEntity = roomEntityReference.Referenced;
        roomEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        RelationshipTestSystem testSystem;
        using (var actorEntityReference = manager.CreateEntity())
        {
            var actorEntity = actorEntityReference.Referenced;
            actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            var being = actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            position.RoomId = roomEntity.Id;
            position.Cell = new Point(2, 3);

            manager.Queue.ProcessQueue(manager);

            testSystem = new RelationshipTestSystem(
                actorEntity, being, manager.RoomActorsToRoomCellRelationship.Dependents);

            manager.Queue.Register<EntityAddedMessage<GameEntity>>(
                testSystem, manager.RoomActorsToRoomCellRelationship.Dependents.GetEntityAddedMessageName(), 10);
            manager.Queue.Register<EntityRemovedMessage<GameEntity>>(
                testSystem, manager.RoomActorsToRoomCellRelationship.Dependents.GetEntityRemovedMessageName(), 10);
            manager.Queue.Register<PropertyValueChangedMessage<GameEntity, int>>(
                testSystem, manager.RoomActorsToRoomCellRelationship.Dependents.GetPropertyValueChangedMessageName(
                    nameof(BeingComponent.EntropyState)), 10);
            manager.RoomActorsToRoomCellRelationship.Dependents.AddListener(testSystem);

            // Linking the actor into the room's cell queues an add; changing a registered property queues a
            // property change; unlinking it (removing its Position) queues a remove.
            actorEntity.Position = position;
            being.EntropyState = 5;
            actorEntity.Position = null;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(3, testSystem.MessagesProcessed);
            Assert.Equal(3, testSystem.GroupChangesDetected);
        }
    }

    [Fact]
    public void Dependent_listener_receives_property_changes()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new DependentTestListener();
        manager.RoomActorsToRoomCellRelationship.AddDependentsListener(listener);

        using var roomEntityReference = manager.CreateEntity();
        var roomEntity = roomEntityReference.Referenced;
        roomEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        var being = actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = roomEntity.Id;
        position.Cell = new Point(2, 3);
        actorEntity.Position = position;

        Assert.Equal(1, listener.AddedCount);
        Assert.Equal(0, listener.PropertyChangedCount);

        being.EntropyState = 5;

        Assert.Equal(1, listener.PropertyChangedCount);
        Assert.Same(actorEntity, listener.LastPropertyChangedEntity);
        Assert.Same(roomEntity, listener.LastPrincipal);
    }

    [Fact]
    public void Dependent_listener_does_not_receive_property_changes_on_removal()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new DependentTestListener();
        manager.RoomActorsToRoomCellRelationship.AddDependentsListener(listener);

        using var roomEntityReference = manager.CreateEntity();
        var roomEntity = roomEntityReference.Referenced;
        roomEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        using var actorEntityReference = manager.CreateEntity();
        var actorEntity = actorEntityReference.Referenced;
        actorEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        actorEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.RoomId = roomEntity.Id;
        position.Cell = new Point(2, 3);
        actorEntity.Position = position;

        Assert.Equal(1, listener.AddedCount);

        // Removing the Position removes the entity from the RoomActors group,
        // which should not fire a property change on the dependent listener.
        actorEntity.Position = null;

        Assert.Equal(0, listener.PropertyChangedCount);
        Assert.Equal(1, listener.RemovedCount);
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
            Assert.Equal(0, message.PropertyChanges.Count);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Same(_group, message.Group);
            Assert.Equal(0, message.PropertyChanges.Count);

            MessagesProcessed++;
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, int> message,
            GameManager state)
        {
            Assert.Same(_testEntity, message.Entity);
            Assert.Equal(_testComponent.ComponentId, message.ChangedComponent?.ComponentId);
            Assert.Same(_testComponent, message.ChangedComponent);
            Assert.Equal(nameof(BeingComponent.EntropyState), message.ChangedPropertyName);
            Assert.Equal(0, message.OldValue);
            Assert.Equal(5, message.NewValue);

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
            Assert.Equal(nameof(BeingComponent.EntropyState), changes.GetChangedPropertyName(0));
            Assert.Equal(0, changes.GetValue<int>(0, ValueType.Old));
            Assert.Equal(5, changes.GetValue<int>(0, ValueType.Current));

            GroupChangesDetected++;
        }
    }
}
