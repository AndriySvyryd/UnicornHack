using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
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

    private static (PlayerComponent Player, LevelChangeBuilder Listener, GameServices Services) SetupGame()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
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

        var playerActor = fresh.Level!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(3, playerActor.LevelY);
    }

    [Fact]
    public void MoveBlocked()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(@"
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
        var playerActor = fresh.Level!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(2, playerActor.LevelX);
        Assert.Equal(2, playerActor.LevelY);
    }

    [Fact]
    public void ChangeHeading()
    {
        var (player, listener, services) = SetupGame();

        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.ChangeHeading, (int)Direction.East);

        var playerActor = fresh.Level!.Actors!.Values.Single(a => a.BaseName == "player");
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
        var level1 = WebTestHelper.BuildLevel(@"
...
.@.
.>.
...", game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level1, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        // Level 2 holds a matching upward connection at (1,1).
        var level2 = WebTestHelper.BuildLevel(@"
...
.<.
...", game);

        var down = level1.Connections.GetValueOrDefault(new Point(1, 2));
        Assert.NotNull(down);
        var up = level2.Connections.GetValueOrDefault(new Point(1, 1));
        Assert.NotNull(up);

        // Wire up bidirectional travel.
        down!.Connection!.TargetLevelId = level2.EntityId;
        down.Connection!.TargetLevelCell = up!.Position!.LevelCell;
        up.Connection!.TargetLevelId = level1.EntityId;
        up.Connection!.TargetLevelCell = down.Position!.LevelCell;

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        // Pre-condition: player on level 1.
        Assert.Equal(level1.EntityId, playerEntity.Position!.LevelId);

        var fresh = WebTestHelper.PerformAndVerify(
            playerEntity.Player!, listener, game.Services,
            ActorActionType.MoveOneCell, (int)Direction.South);

        // Player must have travelled to level 2.
        Assert.Equal(level2.EntityId, playerEntity.Position!.LevelId);
        Assert.Equal(up.Position!.LevelCell, playerEntity.Position!.LevelCell);

        // The fresh client state must reflect level 2's geometry.
        Assert.Equal(level2.Width, fresh.Level!.Width);
        Assert.Equal(level2.Height, fresh.Level!.Height);

        // The player actor in the change set must be on level 2 at the receiving cell.
        var playerActor = fresh.Level!.Actors!.Values.Single(a => a.BaseName == "player");
        Assert.Equal(up.Position!.LevelX, playerActor.LevelX);
        Assert.Equal(up.Position!.LevelY, playerActor.LevelY);
    }
}
