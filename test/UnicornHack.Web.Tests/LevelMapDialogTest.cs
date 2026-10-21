using UnicornHack.Data.Branches;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using System.Buffers;
using Xunit;

namespace UnicornHack.Web.Tests;

public class LevelMapDialogTest
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public void Room_perception_controls_actor_totals_within_non_hidden_graph_range(int perceptionLevel, int range)
    {
        var (player, room) = CreateMapObserver();
        var manager = player.Manager;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            perceptionLevel - player.Being!.RoomPerceptionLevel, nameof(BeingComponent.RoomPerceptionLevel), "test room level", player);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            range - player.Being.RoomPerceptionRange, nameof(BeingComponent.RoomPerceptionRange), "test room range", player);
        manager.Queue.ProcessQueue(manager);
        foreach (var candidate in room.Level.Rooms)
        {
            candidate.IsVisited = candidate == room;
        }

        var map = GameQueryProcessor.SerializeLevelMap(player);
        var expectedIds = new HashSet<int> { room.EntityId };
        if (perceptionLevel > 0)
        {
            for (var distance = 0; distance < range; distance++)
            {
                expectedIds.UnionWith(map.Rooms.Where(entry => expectedIds.Contains(entry.Id))
                    .SelectMany(entry => entry.ConnectedTo).ToArray());
            }
        }

        foreach (var entry in map.Rooms)
        {
            var candidate = room.Level.Rooms.Single(candidate => candidate.EntityId == entry.Id);
            Assert.Equal(expectedIds.Contains(entry.Id) ? candidate.Actors.Values.Count(actor => actor != player) : -1,
                entry.RevealedActorCount);
            Assert.DoesNotContain("Items:", entry.Tooltip);
            Assert.DoesNotContain("Hostiles:", entry.Tooltip);
            Assert.DoesNotContain("Strongest:", entry.Tooltip);
            if (!expectedIds.Contains(entry.Id))
            {
                Assert.Null(entry.EliteName);
                Assert.DoesNotContain("Elite:", entry.Tooltip);
            }
        }

        Assert.Equal(room.Level.Rooms.Count, map.Rooms.Count);
        Assert.Contains(map.Rooms, entry => entry.RevealedActorCount == -1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Visited_rooms_retain_full_details_outside_room_perception_range(int perceptionLevel)
    {
        var (player, room) = CreateMapObserver();
        var manager = player.Manager;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            perceptionLevel - player.Being!.RoomPerceptionLevel,
            nameof(BeingComponent.RoomPerceptionLevel), "test room level", player);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -player.Being.RoomPerceptionRange, nameof(BeingComponent.RoomPerceptionRange), "test room range", player);
        manager.Queue.ProcessQueue(manager);
        foreach (var candidate in room.Level.Rooms)
        {
            candidate.IsVisited = true;
        }

        var map = GameQueryProcessor.SerializeLevelMap(player);

        Assert.Equal(room.Level.Rooms.Count, map.Rooms.Count);
        foreach (var entry in map.Rooms)
        {
            var candidate = room.Level.Rooms.Single(candidate => candidate.EntityId == entry.Id);
            Assert.Equal(candidate.Actors.Values.Count(actor => actor != player), entry.RevealedActorCount);
        }
    }

    private static (GameEntity Player, RoomComponent Room) CreateMapObserver()
    {
        var game = TestHelper.CreateGame(42);
        var branch = BranchData.RuinedStronghold.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(branch, 1, 42, game.Manager);
        LevelGenerator.EnsureGenerated(level);
        var room = level.Rooms.Single(candidate => candidate.FeatureTags.HasFlag(RoomFeatures.Entrance));
        var cell = room.IndexToPoint!.Take(room.TileCount).First(candidate =>
            room.Terrain[room.PointToIndex![candidate.X, candidate.Y]].CanMoveTo()
            && !room.Actors.ContainsKey(candidate) && !room.Connections.ContainsKey(candidate));
        var player = PlayerRace.InstantiatePlayer("Tester", Sex.Male, room, cell);
        game.Manager.Queue.ProcessQueue(game.Manager);
        return (player, room);
    }

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
        Assert.Equal(room.FeatureTags, entry.Features);

        // The server bakes the localized header into the tooltip. The player is standing in
        // this room, so it's the current room.
        Assert.StartsWith("<b>Current room</b>", entry.Tooltip ?? "");
    }

    [Fact]
    public void SerializeLevelMap_identifies_the_elite_but_not_ordinary_actors_when_details_are_perceived()
    {
        var game = TestHelper.CreateGame();
        var room = TestHelper.BuildRoom(".....", game);
        var manager = game.Manager;
        var player = PlayerRace.InstantiatePlayer(
            "Tester", Sex.Male, room, new Point(0, 0));
        var named = CreatureData.Cerberus.Instantiate(room, new Point(1, 0));
        var stronger = CreatureData.Elephant.Instantiate(room, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.True(stronger.Being!.HitPointMaximum > named.Being!.HitPointMaximum);

        var entry = Assert.Single(GameQueryProcessor.SerializeLevelMap(player).Rooms);

        Assert.Equal(2, entry.RevealedActorCount);
        Assert.Contains("Creatures: 2", entry.Tooltip);
        Assert.Equal("Cerberus", entry.EliteName);
        Assert.Contains("Elite: Cerberus", entry.Tooltip);
        Assert.DoesNotContain("elephant", entry.Tooltip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SerializeLevelMap_returns_multiple_rooms_with_connections_for_dungeon_levels()
    {
        var game = TestHelper.CreateGame(seed: 42u);
        var manager = game.Manager;
        var dungeon = BranchData.RuinedStronghold.Instantiate(game);
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
            Assert.Equal(roomComponent.FeatureTags, entry.Features);
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
