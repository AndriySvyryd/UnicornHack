using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
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
        var game = WebTestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var game = WebTestHelper.CreateGame();
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
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var baseline = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));

        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services, ActorActionType.MoveOneCell, (int)Direction.North);

        // Tick should advance even if blocked
        Assert.True(fresh.CurrentTick >= baseline.CurrentTick);

        // Player position must not have moved into the wall.
        var playerActor = fresh.Room!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(2, playerActor.LevelX);
        Assert.Equal(2, playerActor.LevelY);
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
        var game = WebTestHelper.CreateGame();
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

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        // Pre-condition: player on level 1.
        Assert.Equal(level1.EntityId, playerEntity.Position!.RoomId);

        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        // Player must have travelled to level 2.
        Assert.Equal(level2.EntityId, playerEntity.Position!.RoomId);
        Assert.Equal(up.Position!.Cell, playerEntity.Position!.Cell);

        // The fresh client state must reflect level 2's geometry.
        Assert.Equal(level2.Width, fresh.Room!.Width);
        Assert.Equal(level2.Height, fresh.Room!.Height);

        // The player actor in the change set must be on level 2 at the receiving cell.
        var playerActor = fresh.Room!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(up.Position!.CellX, playerActor.LevelX);
        Assert.Equal(up.Position!.CellY, playerActor.LevelY);
    }

    /// <summary>
    ///     Walking onto an intra-level door (a connection whose target room shares the
    ///     same <see cref="GameLevel" />) must publish a <c>SteppedOnConnection</c>
    ///     event in the player's change set with <see cref="ConnectionScope.Room" />.
    /// </summary>
    [Fact]
    public void MoveOntoIntraLevelDoor_EmitsRoomScopedConnectionEvent()
    {
        var game = WebTestHelper.CreateGame();
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
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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
        var single = Assert.Single(allEvents);
        var stepped = Assert.IsType<SteppedOnConnectionEvent>(single);
        Assert.Equal(TurnEventType.SteppedOnConnection, stepped.Type);
        Assert.Equal(ConnectionScope.Room, stepped.Scope);
        Assert.Equal(connectionEntity.Id, stepped.ConnectionId);

        // The player must be standing on the connection tile (intra-level travel stops
        // them on the door rather than carrying them through).
        Assert.Equal(roomA.EntityId, playerEntity.Position!.RoomId);
        Assert.Equal(new Point(1, 2), playerEntity.Position!.Cell);
    }
}
