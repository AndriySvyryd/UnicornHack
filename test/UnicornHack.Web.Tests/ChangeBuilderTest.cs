using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack;

public class ChangeBuilderTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (GameManager Manager, GameEntity PlayerEntity, RoomChangeBuilder Listener)
        SetupGameWithListener()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (manager, playerEntity, listener);
    }

    private static void EnqueueSetSlot(GameManager manager, GameEntity playerEntity, GameEntity abilityEntity, int? slot)
    {
        var msg = SetAbilitySlotMessage.Create(manager);
        msg.AbilityEntity = abilityEntity;
        msg.OwnerEntity = playerEntity;
        msg.Slot = slot;
        manager.Enqueue(msg);
    }

    [Fact]
    public void LogChangesAreTrackedFromAddEvents()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        game.Manager.Queue.ProcessQueue(game.Manager);
        var logEntries = playerEntity.Player!.LogEntries;
        logEntries.Add(new LogEntry { Id = 1, Tick = 1, Message = "baseline" });

        var builder = new LogChangeBuilder(playerEntity);

        Assert.False(builder.HasChanges);
        logEntries.Add(new LogEntry { Id = 3, Tick = 3, Message = "later" });
        logEntries.Add(new LogEntry { Id = 2, Tick = 2, Message = "earlier" });
        Assert.True(builder.HasChanges);
        var serialized = builder.GetSerializedLogEntries();
        Assert.Equal([2, 3], serialized!.Keys);
        Assert.False(builder.HasChanges);

        logEntries.Add(new LogEntry { Id = 4, Tick = 4, Message = "discarded" });
        builder.Reset();
        Assert.False(builder.HasChanges);
        Assert.Null(builder.GetSerializedLogEntries());

        builder.Unregister();
        logEntries.Add(new LogEntry { Id = 5, Tick = 5, Message = "unregistered" });
        Assert.False(builder.HasChanges);
    }

    /// <summary>
    ///     When an ability's slot is cleared and then re-assigned within the same
    ///     change-tracking cycle (Deleted → Added on the same entity), the builder
    ///     should emit a delta — not a full snapshot — because the client already
    ///     knew about this entity from a prior turn.
    /// </summary>
    [Fact]
    public void DeletedThenAdded_ProducesDelta()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        // Equip sword — creates attack ability in slot 0
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        // Capture baseline — client now knows the ability in slot 0
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        var slottedAbility = baseline.Abilities!.Values.Single(a => a.Slot == 0);
        var abilityEntityId = baseline.Abilities.Single(kvp => kvp.Value == slottedAbility).Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;

        // Clear the slot then re-assign it in the same queue cycle (Deleted → Added).
        EnqueueSetSlot(manager, playerEntity, abilityEntity, null);
        EnqueueSetSlot(manager, playerEntity, abilityEntity, 0);
        manager.Queue.ProcessQueue(manager);

        var changeSets = WebTestHelper.PerformAction(player, listener, game.Services, ActorActionType.Wait);
        var abilityDelta = Assert.Single(
            changeSets[playerEntity.Id]
                .SelectMany(changeSet => changeSet.PlayerState.Abilities ?? []),
            change => change.Key == abilityEntityId).Value;
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(3));
        Assert.Equal(0, abilityDelta.Slot);

        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 0);
    }

    [Fact]
    public void SameRoomPlayersReceiveConvergentRoomChanges()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var targetRoom = TestHelper.BuildRoom(TestMap, game);
        targetRoom.Level = room.Level;
        var manager = game.Manager;

        using var connectionReference = manager.CreateEntity();
        var connectionEntity = connectionReference.Referenced;
        var connectionPosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        connectionPosition.RoomEntity = room.Entity;
        connectionPosition.Cell = new Point(2, 1);
        connectionEntity.Position = connectionPosition;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = targetRoom.EntityId;
        connection.TargetCell = new Point(0, 0);
        connection.Directionality = Directionality.Source;
        connection.Orientation = Direction.East;
        connectionEntity.Connection = connection;

        var actingPlayer = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        var observingPlayer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, room, new Point(3, 1));
        var creature = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        ItemData.LongSword.Instantiate(actingPlayer);
        ItemData.LongSword.Instantiate(observingPlayer);
        manager.Queue.ProcessQueue(manager);
        observingPlayer.Being!.HitPoints = 1;
        creature.AI!.NextActionTick = 10_000;

        var listener = WebTestHelper.SetupListeners(manager);
        Assert.Equal(2, listener.PlayerBuilders.Count);

        var actingState = ClientStateHelpers.Deserialize(GameStateManager.GetState(actingPlayer, game.Services));
        var observingState = ClientStateHelpers.Deserialize(GameStateManager.GetState(observingPlayer, game.Services));
        var connectionKnowledgeId = room.KnownConnections.Values.Single(
            knowledge => knowledge.Knowledge!.KnownEntityId == connectionEntity.Id).Id;
        var actingAbility = actingPlayer.Being!.SlottedAbilities.Values.First();
        var observingAbility = observingPlayer.Being!.SlottedAbilities.Values.First();

        actingPlayer.Being!.HitPoints--;
        observingPlayer.Being!.EnergyPoints--;
        creature.AI.NextActionTick++;
        actingAbility.Ability!.Noise = 11;
        observingAbility.Ability!.Noise = 17;
        connection.Orientation = Direction.North;
        actingPlayer.ActiveEffects!.AddPropertyEffect(
            EffectType.BleedDamage,
            combinationFunction: ValueCombinationFunction.Subtract,
            magnitudePrimary: 10,
            expirationEvent: EffectDuration.UntilTimeout,
            expirationTarget: manager.Game.CurrentTick + 100);
        observingPlayer.ActiveEffects!.AddPropertyEffect(
            EffectType.ToxinDamage,
            combinationFunction: ValueCombinationFunction.Subtract,
            magnitudePrimary: 20,
            expirationEvent: EffectDuration.UntilTimeout,
            expirationTarget: manager.Game.CurrentTick + 200);
        manager.LoggingSystem.WriteLog("acting player only", actingPlayer, manager);
        manager.LoggingSystem.WriteLog("observing player only", observingPlayer, manager);
        ItemData.FlaskOfHealing.Instantiate(room, new Point(4, 4));
        manager.Queue.ProcessQueue(manager);
        var sharedItem = room.Items[new Point(4, 4)];

        var changeSets = WebTestHelper.PerformAction(
            actingPlayer.Player!, listener, game.Services, ActorActionType.MoveOneCell, (int)Direction.East);

        Assert.True(changeSets.TryGetValue(actingPlayer.Id, out var actingChanges));
        Assert.True(changeSets.TryGetValue(observingPlayer.Id, out var observingChanges));
        ChangeSetAssert.ApplyAndAssertNoRedundantChanges(actingState, actingChanges!);
        ChangeSetAssert.ApplyAndAssertNoRedundantChanges(observingState, observingChanges!);
        Assert.Contains(actingChanges, changeSet => changeSet.PlayerState.Hp == actingPlayer.Being.HitPoints);
        Assert.Contains(observingChanges, changeSet => changeSet.PlayerState.Ep == observingPlayer.Being.EnergyPoints);
        Assert.Contains(actingState.Log!.Values, entry => entry.Message == "acting player only");
        Assert.DoesNotContain(actingState.Log.Values, entry => entry.Message == "observing player only");
        Assert.Contains(observingState.Log!.Values, entry => entry.Message == "observing player only");
        Assert.DoesNotContain(observingState.Log.Values, entry => entry.Message == "acting player only");
        Assert.Equal(11, actingState.Abilities![actingAbility.Id].Noise);
        Assert.DoesNotContain(observingAbility.Id, actingState.Abilities.Keys);
        Assert.Equal(17, observingState.Abilities![observingAbility.Id].Noise);
        Assert.DoesNotContain(actingAbility.Id, observingState.Abilities.Keys);
        Assert.Contains(actingState.Effects!.Values, effect => effect.EffectType == EffectType.BleedDamage);
        Assert.DoesNotContain(actingState.Effects.Values, effect => effect.EffectType == EffectType.ToxinDamage);
        Assert.Contains(observingState.Effects!.Values, effect => effect.EffectType == EffectType.ToxinDamage);
        Assert.DoesNotContain(observingState.Effects.Values, effect => effect.EffectType == EffectType.BleedDamage);

        var actingEvents = actingChanges.SelectMany(changeSet => changeSet.Events ?? []);
        var actingEvent = Assert.Single(actingEvents);
        Assert.Equal(TurnEventType.SteppedOnConnection, actingEvent.Type);
        Assert.Equal(connectionEntity.Id, actingEvent.Value2);
        Assert.DoesNotContain(observingChanges, changeSet => changeSet.Events != null);

        var sharedItemId = room.KnownItems.Values.Single(
            knowledge => knowledge.Knowledge!.KnownEntityId == sharedItem.Id).Id;
        var actingItemChanges = actingChanges.Select(changeSet => changeSet.PlayerState.Room?.Items)
            .Single(items => items?.ContainsKey(sharedItemId) == true)!;
        var observingItemChanges = observingChanges.Select(changeSet => changeSet.PlayerState.Room?.Items)
            .Single(items => items?.ContainsKey(sharedItemId) == true)!;
        Assert.Same(actingItemChanges, observingItemChanges);
        var actingConnectionChanges = actingChanges.Select(changeSet => changeSet.PlayerState.Room?.Connections)
            .Single(connections => connections?.ContainsKey(connectionKnowledgeId) == true)!;
        var observingConnectionChanges = observingChanges.Select(changeSet => changeSet.PlayerState.Room?.Connections)
            .Single(connections => connections?.ContainsKey(connectionKnowledgeId) == true)!;
        Assert.Same(actingConnectionChanges, observingConnectionChanges);
        var actingPlayerKnowledgeId = actingPlayer.Position!.Knowledge!.Id;
        var actingActorChanges = actingChanges.Select(changeSet => changeSet.PlayerState.Room?.Actors)
            .Single(actors => actors?.ContainsKey(actingPlayerKnowledgeId) == true)!;
        var observingActorChanges = observingChanges.Select(changeSet => changeSet.PlayerState.Room?.Actors)
            .Single(actors => actors?.ContainsKey(actingPlayerKnowledgeId) == true)!;
        Assert.Same(
            actingActorChanges[actingPlayerKnowledgeId],
            observingActorChanges[actingPlayerKnowledgeId]);

        var freshActingState = ClientStateHelpers.Deserialize(GameStateManager.GetState(actingPlayer, game.Services));
        var freshObservingState = ClientStateHelpers.Deserialize(GameStateManager.GetState(observingPlayer, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshActingState, actingState);
        ChangeSetAssert.AssertPlayerStatesEqual(freshObservingState, observingState);
    }

    [Fact]
    public void ConnectionProjectionEmitsMinimalDeltaAndSuppressesRoundTrip()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var targetRoom = TestHelper.BuildRoom(TestMap, game);
        targetRoom.Level = room.Level;
        var manager = game.Manager;

        using var connectionReference = manager.CreateEntity();
        var connectionEntity = connectionReference.Referenced;
        var connectionPosition = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        connectionPosition.RoomEntity = room.Entity;
        connectionPosition.Cell = new Point(2, 1);
        connectionEntity.Position = connectionPosition;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = targetRoom.EntityId;
        connection.TargetCell = new Point(0, 0);
        connection.Directionality = Directionality.Source;
        connection.Orientation = Direction.East;
        connectionEntity.Connection = connection;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);
        var connectionKnowledge = Assert.Single(
            room.KnownConnections.Values,
            knowledge => knowledge.Knowledge!.KnownEntityId == connectionEntity.Id);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        var clientState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));

        connectionEntity.Position.Cell = new Point(2, 2);
        connectionKnowledge.Position!.Cell = new Point(2, 2);

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var connectionChange = Assert.Single(playerChange.Room!.Connections!).Value;
        Assert.NotNull(connectionChange.ChangedProperties);
        Assert.False(connectionChange.ChangedProperties.IsPropertyChanged(1));
        Assert.True(connectionChange.ChangedProperties.IsPropertyChanged(2));
        Assert.False(connectionChange.ChangedProperties.IsPropertyChanged(3));
        Assert.Equal(2, connectionChange.LevelY);

        ClientStateHelpers.ApplyDelta(clientState, playerChange);
        var freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);

        listener.Reset(manager);
        connectionEntity.Position.Cell = new Point(2, 1);
        connectionKnowledge.Position.Cell = new Point(2, 1);
        connectionEntity.Position.Cell = new Point(2, 2);
        connectionKnowledge.Position.Cell = new Point(2, 2);

        Assert.False(playerBuilder.HasObservableChanges(manager));

        listener.Reset(manager);
        connection.Orientation = Direction.North;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var orientationChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var changedConnection = Assert.Single(orientationChange.Room!.Connections!).Value;
        Assert.False(changedConnection.ChangedProperties.IsPropertyChanged(1));
        Assert.False(changedConnection.ChangedProperties.IsPropertyChanged(2));
        Assert.True(changedConnection.ChangedProperties.IsPropertyChanged(3));
        Assert.Equal(Direction.North, changedConnection.Orientation);

        listener.Reset(manager);
        connection.Directionality = Directionality.Destination;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var removalChange = playerBuilder.BuildChangeSet(manager, game.Services);
        Assert.Equal((byte?)0, Assert.Single(removalChange.Room!.Connections!).Value.ChangedProperties);

        listener.Reset(manager);
        connection.Directionality = Directionality.Source;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var restoredChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var restoredConnection = Assert.Single(restoredChange.Room!.Connections!).Value;
        Assert.Null(restoredConnection.ChangedProperties);
        Assert.Equal(2, restoredConnection.LevelX);
        Assert.Equal(2, restoredConnection.LevelY);
        Assert.Equal(Direction.North, restoredConnection.Orientation);
    }

    [Fact]
    public void PickupRemovesItemFromRoomProjection()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(".", game);
        Assert.True(ItemData.FlaskOfHealing.Instantiate(room, new Point(0, 0)));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var state = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.PickupItem);

        Assert.Empty(state.Room!.Items!);
    }

    [Fact]
    public void ItemProjectionEmitsMinimalDeltasAndSuppressesRoundTrip()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        ItemData.FlaskOfHealing.Instantiate(room, new Point(2, 1));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);
        var itemEntity = room.Items[new Point(2, 1)];
        var itemKnowledge = Assert.Single(
            room.KnownItems.Values,
            knowledge => knowledge.Knowledge!.KnownEntityId == itemEntity.Id);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        var clientState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));

        itemEntity.Position!.Cell = new Point(2, 2);
        itemKnowledge.Position!.Cell = new Point(2, 2);

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var positionChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var movedItem = Assert.Single(positionChange.Room!.Items!).Value;
        Assert.NotNull(movedItem.ChangedProperties);
        Assert.False(movedItem.ChangedProperties.IsPropertyChanged(1));
        Assert.True(movedItem.ChangedProperties.IsPropertyChanged(2));
        Assert.False(movedItem.ChangedProperties.IsPropertyChanged(3));
        Assert.False(movedItem.ChangedProperties.IsPropertyChanged(4));
        Assert.False(movedItem.ChangedProperties.IsPropertyChanged(5));
        Assert.False(movedItem.ChangedProperties.IsPropertyChanged(6));
        Assert.Equal(2, movedItem.LevelY);

        ClientStateHelpers.ApplyDelta(clientState, positionChange);
        var freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);

        listener.Reset(manager);
        itemKnowledge.Knowledge!.SensedType = SenseType.None;
        itemKnowledge.Knowledge.SensedType = SenseType.Sight;

        Assert.False(playerBuilder.HasObservableChanges(manager));

        listener.Reset(manager);
        itemEntity.Item!.Count = 2;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var countChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var renamedItem = Assert.Single(countChange.Room!.Items!).Value;
        Assert.NotNull(renamedItem.ChangedProperties);
        Assert.False(renamedItem.ChangedProperties.IsPropertyChanged(1));
        Assert.False(renamedItem.ChangedProperties.IsPropertyChanged(2));
        Assert.False(renamedItem.ChangedProperties.IsPropertyChanged(3));
        Assert.False(renamedItem.ChangedProperties.IsPropertyChanged(4));
        Assert.True(renamedItem.ChangedProperties.IsPropertyChanged(5));
        Assert.False(renamedItem.ChangedProperties.IsPropertyChanged(6));

        ClientStateHelpers.ApplyDelta(clientState, countChange);
        freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);

        listener.Reset(manager);
        itemKnowledge.Knowledge!.IsIdentified = false;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var identificationChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var unidentifiedItem = Assert.Single(identificationChange.Room!.Items!).Value;
        Assert.NotNull(unidentifiedItem.ChangedProperties);
        Assert.True(unidentifiedItem.ChangedProperties.IsPropertyChanged(3));
        Assert.True(unidentifiedItem.ChangedProperties.IsPropertyChanged(4));
        Assert.True(unidentifiedItem.ChangedProperties.IsPropertyChanged(5));
        Assert.Equal(ItemType.None, unidentifiedItem.Type);
        Assert.Null(unidentifiedItem.BaseName);
        Assert.Null(unidentifiedItem.Name);

        ClientStateHelpers.ApplyDelta(clientState, identificationChange);
        freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);
    }

    [Fact]
    public void ItemProjectionEmitsDeletionAndLaterFullSnapshot()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        ItemData.FlaskOfHealing.Instantiate(room, new Point(2, 1));
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);
        var itemEntity = room.Items[new Point(2, 1)];
        var itemKnowledge = Assert.Single(
            room.KnownItems.Values,
            knowledge => knowledge.Knowledge!.KnownEntityId == itemEntity.Id);
        var itemKnowledgeId = itemKnowledge.Id;

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        var clientState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));

        itemKnowledge.Knowledge = null;
        itemKnowledge.Position = null;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var removalChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var removedItem = removalChange.Room!.Items![itemKnowledgeId];
        Assert.Equal((byte?)0, removedItem.ChangedProperties);
        ClientStateHelpers.ApplyDelta(clientState, removalChange);

        var freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);
        Assert.Empty(clientState.Room!.Items!);

        listener.Reset(manager);
        using var restoredKnowledgeReference = manager.CreateEntity();
        var restoredKnowledge = restoredKnowledgeReference.Referenced;
        restoredKnowledge.Knowledge = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
        restoredKnowledge.Knowledge.KnownEntityId = itemEntity.Id;
        restoredKnowledge.Knowledge.SensedType = SenseType.Sight;
        restoredKnowledge.Knowledge.IsIdentified = true;
        restoredKnowledge.Position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        restoredKnowledge.Position.SetRoomPosition(room.EntityId, new Point(2, 1));

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var restoredChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var addedItem = restoredChange.Room!.Items![restoredKnowledge.Id];
        Assert.Null(addedItem.ChangedProperties);
        ClientStateHelpers.ApplyDelta(clientState, restoredChange);

        freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);
        var dropped = Assert.Single(clientState.Room!.Items!.Values);
        Assert.Equal("flask of healing", dropped.BaseName);

        restoredKnowledge.Knowledge = null;
        restoredKnowledge.Position = null;
    }

    [Fact]
    public void ActorProjectionClearsSharedAndAssesmentStateWhenIdentificationIsLost()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        var creature = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);
        var creatureKnowledge = Assert.Single(
            room.KnownActors.Values,
            knowledge => knowledge.Knowledge!.KnownEntityId == creature.Id);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        var clientState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        creatureKnowledge.Knowledge!.IsIdentified = false;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var actorChange = playerChange.Room!.Actors![creatureKnowledge.Id];
        Assert.NotNull(actorChange.ChangedProperties);
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(4));
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(5));
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(7));
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(8));
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(9));
        Assert.True(actorChange.ChangedProperties.IsPropertyChanged(10));
        Assert.Null(actorChange.BaseName);
        Assert.Null(actorChange.Name);
        Assert.Equal(0, actorChange.Hp);
        Assert.Equal(0, actorChange.MaxHp);
        Assert.Equal(0, actorChange.Ep);
        Assert.Equal(0, actorChange.MaxEp);
        Assert.Null(actorChange.NextAction);

        ClientStateHelpers.ApplyDelta(clientState, playerChange);
        var freshState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(freshState, clientState);
    }

    [Fact]
    public void ObserverHitPointsChangeDoesNotInvalidateNextActions()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        CreatureData.HumanWarrior.Instantiate(room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        playerEntity.Being!.HitPoints = 1;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        Assert.True(playerChange.ChangedProperties.IsPropertyChanged(11));
        Assert.DoesNotContain(
            playerChange.Room?.Actors?.Values ?? Enumerable.Empty<ActorChange>(),
            actor => actor.ChangedProperties.IsPropertyChanged(12));
    }

    [Fact]
    public void ObserverExperienceChangeDoesNotInvalidateNextActions()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        CreatureData.HumanWarrior.Instantiate(room, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);

        playerEntity.Being!.ExperiencePoints++;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        Assert.True(playerChange.ChangedProperties.IsPropertyChanged(10));
        Assert.Null(playerChange.Room);
    }

    [Fact]
    public void DefaultAttackSlotChangeDoesNotInvalidateNextActions()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        CreatureData.HumanWarrior.Instantiate(room, new Point(2, 1));
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);
        var sword = playerEntity.Being!.Items.Single(item => item.Item!.TemplateName == "long sword");
        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.PerformAndVerify(
            playerEntity.Player!,
            listener,
            game.Services,
            ActorActionType.EquipItem,
            sword.Id,
            (int)EquipmentSlot.GraspPrimaryMelee);

        var defaultAttack = playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot];
        var playerBuilder = Assert.Single(listener.PlayerBuilders);

        EnqueueSetSlot(manager, playerEntity, defaultAttack, null);
        manager.Queue.ProcessQueue(manager);

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        Assert.NotNull(playerChange.Abilities);
        Assert.DoesNotContain(
            playerChange.Room?.Actors?.Values ?? Enumerable.Empty<ActorChange>(),
            actor => actor.ChangedProperties.IsPropertyChanged(12));
    }

    [Fact]
    public void ObserverMovementPreservesNextActionValue()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        CreatureData.HumanWarrior.Instantiate(room, new Point(3, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);
        var playerKnowledge = playerEntity.Position!.Knowledge!;

        playerEntity.Position.Cell = new Point(1, 2);
        playerKnowledge.Position!.Cell = new Point(1, 2);

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var playerChange = playerBuilder.BuildChangeSet(manager, game.Services);
        var movedPlayer = playerChange.Room!.Actors![playerKnowledge.Id];
        Assert.True(movedPlayer.ChangedProperties.IsPropertyChanged(2));

        Assert.Null(movedPlayer.NextAction);
    }

    [Fact]
    public void FullTerrainDeltaIsSharedButDoesNotAliasWorldArrays()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;
        var actingPlayer = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        var observingPlayer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, room, new Point(3, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        room.KnownTerrainChanges = [];
        room.TerrainChanges = [];
        room.NeighborChanges = [];
        room.VisibleTerrainChanges = [];
        for (short i = 0; i < 16; i++)
        {
            room.KnownTerrain[i] = MapFeature.StoneFloor;
            room.VisibleTerrain[i] = 1;
            room.KnownTerrainChanges[i] = MapFeature.StoneFloor;
            room.VisibleTerrainChanges[i] = 1;
        }

        listener.PrepareCommonRoom(room.Entity, game.Services);
        var actingBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == actingPlayer.Id);
        var observingBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == observingPlayer.Id);
        var actingChange = actingBuilder.BuildChangeSet(manager, game.Services);
        var observingChange = observingBuilder.BuildChangeSet(manager, game.Services);
        var actingMap = actingChange.Room!.LevelMap!;
        var observingMap = observingChange.Room!.LevelMap!;

        Assert.Same(actingMap, observingMap);
        Assert.NotSame(room.KnownTerrain, actingMap.Terrain);
        Assert.NotSame(room.VisibleTerrain, actingMap.VisibleTerrain);
        Assert.Equal(room.TileCount, actingMap.Terrain.Length);
        Assert.Equal(room.TileCount, actingMap.WallNeighbors.Length);
        Assert.Equal(room.TileCount, actingMap.VisibleTerrain.Length);

        var terrain = actingMap.Terrain[0];
        var visibility = actingMap.VisibleTerrain[0];
        room.KnownTerrain[0] = MapFeature.Unexplored;
        room.VisibleTerrain[0] = 0;

        Assert.Equal(terrain, actingMap.Terrain[0]);
        Assert.Equal(visibility, actingMap.VisibleTerrain[0]);
    }

    [Fact]
    public void KnownNeighborsCannotReplaceExplorationMaskedNeighbors()
    {
        var room = TestHelper.BuildRoom(TestMap);
        room.KnownTerrain[0] = MapFeature.Unexplored;
        room.Neighbors[0] = DirectionFlags.North;
        room.KnownNeighbors[0] = DirectionFlags.North;
        room.KnownTerrain[1] = MapFeature.StoneFloor;
        room.Neighbors[1] = DirectionFlags.South;
        room.KnownNeighbors[1] = DirectionFlags.South;

        var masked = TerrainChangeBuilder.MaskWallNeighborsToExplored(room);

        Assert.Equal(room.TileCount, masked.Length);
        Assert.Equal(DirectionFlags.None, masked[0]);
        Assert.Equal(DirectionFlags.South, masked[1]);
        Assert.NotEqual(room.KnownNeighbors, masked);
    }

    [Fact]
    public void FullRoomSnapshotsShareCommonStateButOwnActorViews()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(TestMap, game);
        var otherRoom = TestHelper.BuildRoom(TestMap, game);
        otherRoom.Level = room.Level;
        otherRoom.GridX = 1;
        var manager = game.Manager;
        var firstPlayer = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(1, 1));
        var secondPlayer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, room, new Point(3, 1));
        CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        ItemData.FlaskOfHealing.Instantiate(room, new Point(4, 4));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var first = RoomChangeBuilder.SerializeRoom(
            room.Entity,
            new SerializationContext(null!, firstPlayer, game.Services), listener);
        var second = RoomChangeBuilder.SerializeRoom(
            room.Entity,
            new SerializationContext(null!, secondPlayer, game.Services), listener);

        Assert.NotSame(first.Actors, second.Actors);
        Assert.Same(first.Items, second.Items);
        Assert.Same(first.Connections, second.Connections);
        Assert.Same(first.LevelMap, second.LevelMap);
        Assert.Same(first.LevelMapChanges, second.LevelMapChanges);

        var map = first.LevelMap!;
        Assert.NotSame(room.KnownTerrain, map.Terrain);
        Assert.NotSame(room.VisibleTerrain, map.VisibleTerrain);
        var terrain = map.Terrain[0];
        var visibility = map.VisibleTerrain[0];
        room.KnownTerrain[0] = MapFeature.Unexplored;
        room.VisibleTerrain[0] = 0;

        Assert.Equal(terrain, map.Terrain[0]);
        Assert.Equal(visibility, map.VisibleTerrain[0]);

        var other = RoomChangeBuilder.SerializeRoom(
            otherRoom.Entity,
            new SerializationContext(null!, firstPlayer, game.Services), listener);
        Assert.Equal(otherRoom.GridX, other.GridX);
        Assert.NotSame(first.Items, other.Items);
        Assert.NotSame(first.Connections, other.Connections);
        Assert.NotSame(first.LevelMap, other.LevelMap);
    }

    /// <summary>
    ///     A brand-new ability gains a slot then loses it within a single cycle.
    ///     The client never saw it, so the entry must be cancelled — no emission,
    ///     and the ability must not appear in the resulting state.
    /// </summary>
    [Fact]
    public void AddedThenRemoved_Cancels()
    {
        // Custom setup: clear the auto-slotted flask ability before the listener is
        // registered, so the client's initial state does not contain it.
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var ability = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "flask of healing: Drink")]!;
        Assert.NotNull(ability);
        ability.Ability!.Slot = null;

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var player = playerEntity.Player!;

        Assert.DoesNotContain(ability.Id,
            ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services)).Abilities!.Keys);

        // Queue Slot=N then Slot=null on the same ability so both transitions are
        // processed within one Turn. The listener observes Add + Remove on an entity
        // the client did not know about — the entry must be cancelled rather than
        // emitting a removal sentinel for an unknown id.
        EnqueueSetSlot(manager, playerEntity, ability, 3);
        EnqueueSetSlot(manager, playerEntity, ability, null);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        Assert.DoesNotContain(ability.Id, fresh.Abilities!.Keys);
    }

    /// <summary>
    ///     A known ability has a property change followed by slot clearing in the
    ///     same cycle. Emission should be a removal sentinel — the property
    ///     change is irrelevant once the ability leaves the client's view.
    /// </summary>
    [Fact]
    public void ModifiedThenRemoved_EmitsRemovalSentinel()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        var slottedAbility = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services))
            .Abilities!.Single(kvp => kvp.Value.Slot == 0);
        var abilityEntityId = slottedAbility.Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;

        // Mutate a property directly and queue the slot-clear so it is
        // processed in the upcoming Turn. PerformAndVerify's baseline (captured
        // before Turn runs) contains the ability with the new TargetingShapeSize;
        // the change set must then emit a removal sentinel — the property bit is
        // irrelevant once the ability leaves the client's view.
        // (CooldownTick can't be used here because SetAbilitySlotMessage gates on
        // it: slot=null with a non-null cooldown returns StopProcessing without
        // clearing the slot.)
        abilityEntity.Ability!.TargetingShapeSize += 7;
        EnqueueSetSlot(manager, playerEntity, abilityEntity, null);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        Assert.DoesNotContain(abilityEntityId, fresh.Abilities!.Keys);
    }

    /// <summary>
    ///     Clear slot, change a property, restore slot in one cycle. Event masks retain
    ///     both touched properties, including the round-tripped slot value.
    /// </summary>
    [Fact]
    public void ReAddedWithPropertyChange_PreservesTouchedBits()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        // Take baseline before mutations so the emitted delta can be applied and
        // compared with a fresh snapshot.
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        var slottedAbility = baseline.Abilities!.Values.Single(a => a.Slot == 0);
        var abilityEntityId = baseline.Abilities.Single(kvp => kvp.Value == slottedAbility).Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;
        var originalSize = abilityEntity.Ability!.TargetingShapeSize;

        // Clear slot, change a non-gating property, restore slot — all in one cycle.
        abilityEntity.Ability!.Slot = null;
        abilityEntity.Ability!.TargetingShapeSize = originalSize + 7;
        abilityEntity.Ability!.Slot = 0;

        var changeSets = WebTestHelper.PerformAction(player, listener, game.Services, ActorActionType.Wait);
        var playerChangeSets = changeSets[playerEntity.Id];

        // Locate the ability delta in the emitted change sets.
        AbilityChange? abilityDelta = null;
        foreach (var changeSet in playerChangeSets)
        {
            if (changeSet.PlayerState.Abilities is { } abilities
                && abilities.TryGetValue(abilityEntityId, out var d))
            {
                abilityDelta = d;
                break;
            }
        }

        Assert.NotNull(abilityDelta);
        Assert.NotNull(abilityDelta!.ChangedProperties);
        Assert.False(abilityDelta.ChangedProperties == 0,
            "Expected a delta, not a removal sentinel.");
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(3),
            "Slot bit should be retained because the property was touched.");
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(7),
            "TargetingShapeSize bit should be preserved (genuinely changed).");

        // Apply and verify final state aligns with a fresh server snapshot.
        ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets);
        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        Assert.Equal(0, fresh.Abilities![abilityEntityId].Slot);
        Assert.Equal(originalSize + 7, fresh.Abilities![abilityEntityId].TargetingShapeSize);
    }
}
