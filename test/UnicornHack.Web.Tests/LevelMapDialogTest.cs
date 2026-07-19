using UnicornHack.Data.Branches;
using UnicornHack.Generation;
using UnicornHack.Hubs;
using Xunit;

namespace UnicornHack.Web.Tests;

public class LevelMapDialogTest
{
    [Fact]
    public void SerializeLevelMap_returns_a_single_entry_for_test_levels()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(@"
...
.>.
...", game);
        var manager = game.Manager;
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, new Point(1, 1));
        manager.Queue.ProcessQueue(manager);

        var result = GameQueryProcessor.SerializeLevelMap(player);

        Assert.Single(result.Rooms);
        var entry = result.Rooms[0];
        Assert.Equal(room.EntityId, entry.Id);
        Assert.Equal(room.Features, entry.Features);

        // The server bakes the localized header into the tooltip. The player is standing in
        // this room, so it's the current room.
        Assert.StartsWith("<b>Current room</b>", entry.Tooltip ?? "");
    }

    [Fact]
    public void SerializeLevelMap_returns_multiple_rooms_with_connections_for_dungeon_levels()
    {
        var game = TestHelper.CreateGame(seed: 42u);
        var manager = game.Manager;
        var dungeon = BranchData.Dungeon.Instantiate(game);
        game.Branches.Add(dungeon);

        var gameLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 42u, manager);
        LevelGenerator.EnsureGenerated(gameLevel);
        manager.Queue.ProcessQueue(manager);

        // Pick the first walkable cell in any generated room so the player spawn is always
        // valid regardless of room dimensions or interior layout. Skip cells already taken
        // by a connection or actor — terrain can be `CanMoveTo` but still host a feature
        // (stairs, fountain, …) that conflicts with the new player position.
        var (room, spawn) = gameLevel.Rooms
            .SelectMany(r => Enumerable.Range(0, r.Height)
                .SelectMany(y => Enumerable.Range(0, r.Width).Select(x => (r, new Point((byte)x, (byte)y)))))
            .First(t =>
                t.r.Terrain[t.Item2.X + t.Item2.Y * t.r.Width].CanMoveTo()
                && !t.r.Connections.ContainsKey(t.Item2)
                && !t.r.Actors.ContainsKey(t.Item2));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, spawn);
        manager.Queue.ProcessQueue(manager);

        var result = GameQueryProcessor.SerializeLevelMap(player);

        Assert.True(result.Rooms.Count > 1,
            $"Expected multiple rooms for a dungeon level, got {result.Rooms.Count}");

        // All rooms should have their features revealed and connections matching the room graph.
        var totalConnections = 0;
        foreach (var entry in result.Rooms)
        {
            var roomComponent = gameLevel.Rooms.First(r => r.EntityId == entry.Id);
            Assert.Equal(roomComponent.Features, entry.Features);
            Assert.Equal(roomComponent.GridX, entry.GridX);
            Assert.Equal(roomComponent.GridY, entry.GridY);

            // Each serialized connection should match a non-hidden intra-level ConnectionComponent.
            foreach (var targetId in entry.ConnectedTo)
            {
                var hasConnection = roomComponent.Connections.Values
                    .Any(c =>
                    {
                        var conn = c.Connection!;
                        var target = conn.TargetRoomEntity.Room;
                        return target != null
                            && target.EntityId == targetId
                            && target.Level == roomComponent.Level
                            && !conn.Hidden;
                    });
                Assert.True(hasConnection,
                    $"Room {entry.Id} ({entry.GridX},{entry.GridY}) claims connection to {targetId} but no matching ConnectionComponent found");
            }

            totalConnections += entry.ConnectedTo.Count;
        }

        // The graph must have at least one connection (entrance → something).
        Assert.True(totalConnections > 0, "Expected at least one connection in the level map");

        // Must have entrance and exit rooms.
        Assert.Contains(result.Rooms, r => r.Features.HasFlag(RoomFeatures.Entrance));
        Assert.Contains(result.Rooms, r => r.Features.HasFlag(RoomFeatures.Exit));
    }
}
