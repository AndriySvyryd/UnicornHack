using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack;

public class MovementTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (PlayerComponent Player, RoomChangeBuilder Listener, GameServices Services) SetupGame()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (playerEntity.Player!, listener, game.Services);
    }

    [Fact]
    public void MoveOneCell()
    {
        var (player, listener, services) = SetupGame();

        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.MoveOneCell, (int)Direction.South);

        var playerActor = fresh.Room!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(3, playerActor.LevelY);
    }

    [Fact]
    public void MoveBlocked()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(@"
.....
..#..
..@..
.....
.....", game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        Assert.True(baseline.Room!.Actors!.Values.Single(a => a.BaseName == "player").IsCurrentlyPerceived);
        Assert.True(playerEntity.Position!.Knowledge!.Knowledge!.SensedType.CanIdentify());

        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.MoveOneCell, (int)Direction.North);
        Assert.True(playerEntity.Position!.Knowledge!.Knowledge!.SensedType.CanIdentify());

        // Tick should advance even if blocked
        Assert.True(fresh.CurrentTick >= baseline.CurrentTick);

        // Player position must not have moved into the wall.
        var playerActor = fresh.Room!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(2, playerActor.LevelX);
        Assert.Equal(2, playerActor.LevelY);
        Assert.True(playerActor.IsCurrentlyPerceived);
    }

    [Fact]
    public void ChangeHeading()
    {
        var (player, listener, services) = SetupGame();

        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.ChangeHeading, (int)Direction.East);

        var playerActor = fresh.Room!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal((byte)Direction.East, playerActor.Heading);
    }

    [Fact]
    public void Wait()
    {
        var (player, listener, services) = SetupGame();

        Assert.Equal(0, player.Game.CurrentTick);

        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.Wait);

        Assert.True(fresh.CurrentTick > 0);
    }

    /// <summary>
    ///     Walking onto a connection cell triggers a level transition. Verifies the
    ///     change tracker emits a coherent change set covering the full re-render of
    ///     the new level (terrain, actors, items, connections).
    /// </summary>
    [Fact]
    public void MoveOntoConnection_TransitionsToTargetLevel()
    {
        var game = TestHelper.CreateGame();
        var manager = game.Manager;

        // Level 1 holds a downward connection at (1,2). Player starts north of it.
        var level1 = TestHelper.BuildRoom(@"
...
.@.
.>.
...", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        // Level 2 holds a matching upward connection at (1,1).
        var level2 = TestHelper.BuildRoom(@"
...
.<.
...", game);

        var down = level1.Connections.GetValueOrDefault(new Point(1, 2));
        Assert.NotNull(down);
        var up = level2.Connections.GetValueOrDefault(new Point(1, 1));
        Assert.NotNull(up);

        // Wire up bidirectional travel.
        down!.Connection!.TargetRoomId = level2.EntityId;
        down.Connection!.TargetCell = up!.Position!.Cell;
        up.Connection!.TargetRoomId = level1.EntityId;
        up.Connection!.TargetCell = down.Position!.Cell;

        // The cell the player lands on in level 2. level1 is a different branch from level2, so
        // travellng there deletes level1 entirely — and with it the up-stair (an incoming
        // connection of level1), so capture the receiving cell before the move.
        var receivingCell = up.Position!.Cell!.Value;

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        // Pre-condition: player on level 1.
        Assert.Equal(level1.EntityId, playerEntity.Position!.RoomId);

        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        // Player must have travelled to level 2.
        Assert.Equal(level2.EntityId, playerEntity.Position!.RoomId);
        Assert.Equal(receivingCell, playerEntity.Position!.Cell);

        // The fresh client state must reflect level 2's geometry.
        Assert.Equal(level2.Width, fresh.Room!.Width);
        Assert.Equal(level2.Height, fresh.Room!.Height);

        // The player actor in the change set must be on level 2 at the receiving cell.
        var playerActorEntry = fresh.Room!.Actors!.Single(a => a.Value.BaseName == "player");
        var playerActor = playerActorEntry.Value;
        Assert.Equal(playerActorEntry.Key, fresh.Id);
        Assert.Equal(playerEntity.Position.Knowledge!.Id, fresh.Id);
        Assert.Equal(receivingCell.X, playerActor.LevelX);
        Assert.Equal(receivingCell.Y, playerActor.LevelY);
    }

    /// <summary>
    ///     Regression: the current room's grid coordinates
    ///     (<see cref="RoomChange.GridX" /> / <see cref="RoomChange.GridY" />) must update
    ///     when the player transitions to a new room. They were previously stuck at the
    ///     default (0,0) on the client because the room snapshot's GridX/GridY (keys 10/11)
    ///     were never deserialized/applied, which made the map screen highlight the wrong
    ///     (top-left) cell as the current room.
    /// </summary>
    [Fact]
    public void RoomGridCoordinates_UpdateOnTransition()
    {
        var game = TestHelper.CreateGame();
        var manager = game.Manager;

        var level1 = TestHelper.BuildRoom(@"
...
.@.
.>.
...", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var level2 = TestHelper.BuildRoom(@"
...
.<.
...", game);

        // Give the target room non-(0,0) grid coordinates so a never-applied value (the bug
        // left them at the default 0) is detectable.
        level2.GridX = 3;
        level2.GridY = 5;

        var down = level1.Connections.GetValueOrDefault(new Point(1, 2));
        Assert.NotNull(down);
        var up = level2.Connections.GetValueOrDefault(new Point(1, 1));
        Assert.NotNull(up);

        down!.Connection!.TargetRoomId = level2.EntityId;
        down.Connection!.TargetCell = up!.Position!.Cell;
        up.Connection!.TargetRoomId = level1.EntityId;
        up.Connection!.TargetCell = down.Position!.Cell;

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        // Baseline: player on level 1, whose grid coordinates are the default (0,0).
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        Assert.Equal(level1.GridX, baseline.Room!.GridX);
        Assert.Equal(level1.GridY, baseline.Room!.GridY);

        // Walk south onto the connection -> transition to level 2.
        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        Assert.Equal(level2.EntityId, playerEntity.Position!.RoomId);

        // The client's current room must now carry level 2's grid coordinates, compared
        // against the known server values (not another client-derived value, which would be
        // equally stale and hide the bug).
        Assert.Equal(level2.GridX, fresh.Room!.GridX);
        Assert.Equal(level2.GridY, fresh.Room!.GridY);
    }

    /// <summary>
    ///     Walking onto an intra-level door (a connection whose target room shares the
    ///     same <see cref="GameLevel" />) must publish a <c>SteppedOnConnection</c>
    ///     event in the player's change set with <see cref="ConnectionScope.Room" />.
    /// </summary>
    [Fact]
    public void MoveOntoIntraLevelDoor_EmitsRoomScopedConnectionEvent()
    {
        var game = TestHelper.CreateGame();
        var manager = game.Manager;

        // Two rooms wired into a single GameLevel. The player starts adjacent to a
        // doorway in roomA pointing into roomB.
        var roomA = TestHelper.BuildRoom(@"
...
...
...", game);
        var roomB = TestHelper.BuildRoom(@"
...
...
...", game);

        // Force both rooms onto the same GameLevel so the connection counts as intra-level.
        roomB.Level = roomA.Level;

        // Manually create a one-way intra-level connection from roomA(1,2) -> roomB(1,0).
        using var connectionEntityReference = manager.CreateEntity();
        var connectionEntity = connectionEntityReference.Referenced;
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = roomA.Entity;
        position.Cell = new Point(1, 2);
        connectionEntity.Position = position;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = roomB.EntityId;
        connection.TargetCell = new Point(1, 0);
        connection.Directionality = Directionality.Source;
        connection.Orientation = Direction.South;
        connectionEntity.Connection = connection;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, roomA, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var changeSets = WebTestHelper.PerformAction(
            playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        Assert.True(changeSets.TryGetValue(playerEntity.Id, out var playerSets));
        var allEvents = playerSets!
            .Where(cs => cs.Events != null)
            .SelectMany(cs => cs.Events!)
            .ToList();

        // Exactly one event, of type SteppedOnConnection, scope Room, pointing at our
        // freshly-created connection entity.
        var stepped = Assert.Single(allEvents);
        Assert.Equal(TurnEventType.SteppedOnConnection, stepped.Type);
        Assert.Equal((int)ConnectionScope.Room, stepped.Value1);
        Assert.Equal(connectionEntity.Id, stepped.Value2);

        // The player must be standing on the connection tile (intra-level travel stops
        // them on the door rather than carrying them through).
        Assert.Equal(roomA.EntityId, playerEntity.Position!.RoomId);
        Assert.Equal(new Point(1, 2), playerEntity.Position!.Cell);
    }

    /// <summary>
    ///     Regression: after the player leaves a room, creatures left behind keep taking
    ///     turns. Their per-turn property changes (e.g. <c>NextActionTick</c>) are routed by
    ///     the shared change builder to their knowledge entities, which live in the abandoned
    ///     room. Those changes must NOT leak into the change set of a player who is now in a
    ///     different room — the client received a fresh snapshot of the new room and never
    ///     knew those actors, so a stray <c>Modified</c> delta would throw
    ///     "Actor &lt;id&gt; not found" while applying the change set.
    /// </summary>
    [Fact]
    public void ActorChangesInAbandonedRoom_DoNotLeakIntoTheNewRoomChangeSet()
    {
        var game = TestHelper.CreateGame();
        var manager = game.Manager;

        // Level 1 holds a downward connection at (1,2) and a creature the player can see.
        var level1 = TestHelper.BuildRoom(@"
....
.@..
.>..
....", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        // A creature adjacent to the player (so it is perceived and a knowledge entity is
        // created), which stays behind on level 1 when the player descends.
        CreatureData.HumanWarrior.Instantiate(level1, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);

        // Level 2 holds a matching upward connection at (1,1).
        var level2 = TestHelper.BuildRoom(@"
...
.<.
...", game);

        var down = level1.Connections.GetValueOrDefault(new Point(1, 2));
        Assert.NotNull(down);
        var up = level2.Connections.GetValueOrDefault(new Point(1, 1));
        Assert.NotNull(up);

        down!.Connection!.TargetRoomId = level2.EntityId;
        down.Connection!.TargetCell = up!.Position!.Cell;
        up.Connection!.TargetRoomId = level1.EntityId;
        up.Connection!.TargetCell = down.Position!.Cell;

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        // Take a turn so the player's field of view is computed and the creature becomes a
        // known actor in the client's level-1 view.
        WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.ChangeHeading, (int)Direction.East);
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        Assert.Contains(baseline.Room!.Actors!.Values, a => a.BaseName != "player");

        // Walk south onto the connection -> transition to level 2 (full snapshot, no creature).
        WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.MoveOneCell, (int)Direction.South);
        Assert.Equal(level2.EntityId, playerEntity.Position!.RoomId);

        // Now wait on level 2. The level-1 creature keeps taking turns; its change-set entries
        // must be filtered out. PerformAndVerify applies the deltas to the client baseline and
        // would throw "Actor <id> not found" if a cross-room change leaked through.
        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.Wait);

        // The level-2 view must contain only the player; the abandoned creature never appears.
        Assert.DoesNotContain(fresh.Room!.Actors!.Values, a => a.BaseName != "player");
    }

    [Fact]
    public void Known_actor_room_change_is_deleted_then_added_for_the_observer_room()
    {
        var game = TestHelper.CreateGame();
        var manager = game.Manager;
        var observedRoom = TestHelper.BuildRoom(TestMap, game);
        var otherRoom = TestHelper.BuildRoom(TestMap, game);
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, observedRoom, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var actorEntity = CreatureData.HumanWarrior.Instantiate(observedRoom, new Point(2, 1));
        manager.Queue.ProcessQueue(manager);
        using var knowledgeReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeReference.Referenced;
        knowledgeEntity.Knowledge = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
        knowledgeEntity.Knowledge.KnownEntityId = actorEntity.Id;
        knowledgeEntity.Knowledge.SensedType = SenseType.Sight;
        knowledgeEntity.Knowledge.IsIdentified = true;
        knowledgeEntity.Position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        knowledgeEntity.Position.SetRoomPosition(observedRoom.EntityId, new Point(2, 1));
        knowledgeEntity.Position.Heading = Direction.East;

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);
        var playerBuilder = Assert.Single(listener.PlayerBuilders);

        knowledgeEntity.Position.RoomId = otherRoom.EntityId;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var movedOut = playerBuilder.BuildChangeSet(manager, game.Services);
        Assert.Equal((ushort?)0, movedOut.Room!.Actors![knowledgeEntity.Id].ChangedProperties);

        listener.Reset(manager);
        knowledgeEntity.Position.RoomId = observedRoom.EntityId;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var movedIn = playerBuilder.BuildChangeSet(manager, game.Services);
        var added = movedIn.Room!.Actors![knowledgeEntity.Id];
        Assert.Null(added.ChangedProperties);
        Assert.Equal(2, added.LevelX);
        Assert.Equal(1, added.LevelY);

        var clientState = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, game.Services));
        listener.Reset(manager);

        knowledgeEntity.Position.RoomId = otherRoom.EntityId;
        knowledgeEntity.Position.Cell = new Point(3, 2);
        knowledgeEntity.Position.RoomId = observedRoom.EntityId;

        Assert.True(playerBuilder.HasObservableChanges(manager));
        var roundTrip = playerBuilder.BuildChangeSet(manager, game.Services);
        var roundTripActor = roundTrip.Room!.Actors![knowledgeEntity.Id];
        Assert.NotNull(roundTripActor.ChangedProperties);
        Assert.Equal(3, roundTripActor.LevelX);
        Assert.Equal(2, roundTripActor.LevelY);

        ClientStateHelpers.ApplyDelta(clientState, roundTrip);
        var clientActor = clientState.Room!.Actors![knowledgeEntity.Id];
        Assert.Equal(3, clientActor.LevelX);
        Assert.Equal(2, clientActor.LevelY);

        knowledgeEntity.Position = null;
        knowledgeEntity.Knowledge = null;
    }
}
