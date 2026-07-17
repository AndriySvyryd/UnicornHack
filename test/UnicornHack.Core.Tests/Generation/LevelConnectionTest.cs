using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class LevelConnectionTest
{
    /// <summary>
    ///     Regression: with seed 1 the entrance level needs two generation attempts. After a
    ///     failed attempt, <see cref="LevelGenerator.ResetGeneratedState"/> leaves the entrance
    ///     room's incoming connection from the surface already wired (non-dangling). On the
    ///     retry, the entrance room's dynamically placed connecting fragment used to fall
    ///     through to <see cref="LevelConnection.CreateSourceConnection"/> and spuriously source
    ///     a forward connection to the <c>dungeon</c> branch — even though the entrance room is
    ///     not the level's exit. Inter-level connections must only be <em>sourced</em> (forward,
    ///     <see cref="Direction.Down"/>) by dynamic fragments from exit-tagged rooms; static maps
    ///     that explicitly author a connection (a glyph) are exempt. The legitimate Up-return
    ///     connection a room receives from the level above (orientation <see cref="Direction.Up"/>)
    ///     is not a source and is only allowed on entrance rooms .
    /// </summary>
    [Fact]
    public void Dynamic_fragments_source_inter_level_connections_only_from_exit_rooms()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var surfaceRoom = surfaceLevel.Rooms.First();
        var entranceRoom = surfaceRoom.Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var entranceLevel = entranceRoom.Level;

        foreach (var room in entranceLevel.Rooms)
        {
            if ((room.Features & RoomFeatures.Exit) != 0)
            {
                continue;
            }

            // Only forward-sourced connections (orientation Down) are the bug; a received
            // Up-return to the level above is legitimate on the entrance room.
            var sourcedInterLevel = room.Connections.Values
                .Select(c => c.Connection!)
                .Where(c => c.Orientation == Direction.Down
                    && c.TargetRoomEntity.Room is { } target && target.Level != room.Level)
                .Select(c => $"{c.Entity.Position!.Cell} -> {c.TargetRoomEntity.Room!.Level.BranchName}:{c.TargetRoomEntity.Room!.Level.Depth}")
                .ToList();

            Assert.True(sourcedInterLevel.Count == 0,
                $"Non-exit room (features={room.Features}) at grid ({room.GridX},{room.GridY}) "
                + $"sources inter-level connections: {string.Join(", ", sourcedInterLevel)}");
        }

        // The exit room still sources exactly one inter-level connection to the dungeon.
        var exitRoom = entranceLevel.Rooms.Single(r => (r.Features & RoomFeatures.Exit) != 0);
        var exitOutbound = exitRoom.Connections.Values
            .Select(c => c.Connection!)
            .Where(c => c.Orientation == Direction.Down
                && c.TargetRoomEntity.Room is { } target && target.Level != exitRoom.Level)
            .ToList();
        Assert.Single(exitOutbound);
    }

    /// <summary>
    ///     The room pre-created by a neighboring level's connection (the placeholder that
    ///     receives the incoming cross-level connection) must be the level's entrance, must be
    ///     the <em>only</em> entrance, and must keep the flag through generation (including any
    ///     <see cref="LevelGenerator.ResetGeneratedState"/> retry that resets and reuses the
    ///     placeholder). The invariant holds regardless of how many attempts the layout needs.
    /// </summary>
    [Fact]
    public void Pre_created_room_is_the_sole_entrance_and_keeps_the_flag_across_retries()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        // The placeholder pre-created on the entrance level by the surface's '>' connection.
        var surfaceConnection = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!;
        var preCreatedRoom = surfaceConnection.TargetRoomEntity.Room!;

        // It carries the Entrance flag from creation, before the entrance level is generated.
        Assert.True((preCreatedRoom.Features & RoomFeatures.Entrance) != 0);

        var entranceLevel = preCreatedRoom.Level;
        LevelGenerator.EnsureGenerated(preCreatedRoom);

        // The pre-created room is still the entrance after generation, and it received the
        // surface's connection (its incoming connection's target cell is resolved).
        Assert.True((preCreatedRoom.Features & RoomFeatures.Entrance) != 0);
        Assert.NotNull(surfaceConnection.TargetCell);

        // Exactly one room on the level is the entrance — the pre-created one.
        var entranceRooms = entranceLevel.Rooms.Where(r => (r.Features & RoomFeatures.Entrance) != 0).ToList();
        Assert.Single(entranceRooms);
        Assert.Same(preCreatedRoom, entranceRooms[0]);
    }

    /// <summary>
    ///     Regression: when the surface level is generated first and then the dungeon is
    ///     generated on demand (the production <c>GameHub.CreatePlayer</c> flow), the
    ///     surface's outbound <c>&gt;</c> connection's <see cref="ConnectionComponent.TargetCell"/>
    ///     must be populated by the dungeon's entrance fragment processing the matching
    ///     <c>&lt;</c> glyph. This used to fail because <c>MaterializeGraph</c> created fresh
    ///     rooms for every graph node and orphaned the placeholder room created earlier by
    ///     <see cref="LevelConnection.CreateSourceConnection"/>; the surface's incoming
    ///     connection on the placeholder was never visited by the new entrance room's
    ///     <see cref="ConnectingMapFragment.CreateConnection"/> loop. The fix has
    ///     <see cref="LevelLayout.GetEmptyEntranceRoom"/> reuse the placeholder for the
    ///     entrance node so its <c>IncomingConnections</c> are seen by the new room.
    /// </summary>
    [Fact]
    public void Surface_to_dungeon_connection_TargetCell_is_resolved_after_dungeon_generation()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var surfaceRoom = surfaceLevel.Rooms.First();
        var outbound = surfaceRoom.Connections.Single().Value.Connection!;

        // Before the entrance is generated, only the placeholder room exists on the
        // entrance level — TargetCell is null because the entrance cell isn't picked yet.
        Assert.Null(outbound.TargetCell);
        var placeholderRoom = outbound.TargetRoomEntity.Room!;
        Assert.Equal(0, placeholderRoom.Width);

        // Generating the entrance must (a) populate the surface's TargetCell, (b) leave the
        // surface's outbound connection pointing at the real (now non-empty) entrance room.
        LevelGenerator.EnsureGenerated(placeholderRoom);

        Assert.NotNull(outbound.TargetCell);
        var entranceRoom = outbound.TargetRoomEntity.Room!;
        Assert.True(entranceRoom.Width > 0, "Entrance room should have allocated terrain.");
        Assert.True((entranceRoom.Features & RoomFeatures.Entrance) != 0,
            "TargetRoomEntity should be the entrance branch's entrance room.");
        Assert.True(entranceRoom.IsValid(outbound.TargetCell!.Value),
            "TargetCell must lie inside the entrance room.");
    }

    /// <summary>
    ///     Regression: <see cref="Branch.Instantiate"/> used to create a fresh
    ///     <see cref="GameBranch"/> without registering it on <see cref="Game.Branches"/>,
    ///     so <see cref="Game.GetBranch"/> kept returning <c>null</c>. Every <c>&gt;</c>
    ///     glyph in the dungeon's defining map then ran the placeholder-creation branch of
    ///     <see cref="LevelConnection.CreateSourceConnection"/> and produced a duplicate
    ///     <see cref="GameBranch"/> + dungeon depth-2 <see cref="GameLevel"/> + placeholder
    ///     <see cref="RoomComponent"/> per glyph (the symptom upstream was a SQL FK
    ///     violation when EF deduplicated the levels by composite key but the placeholder
    ///     rooms still pointed at orphaned level instances).
    /// </summary>
    [Fact]
    public void Cross_branch_connections_dedupe_to_a_single_branch_and_level()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.Entrance.Instantiate(game);
        BranchData.Dungeon.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        // surface → entrance (single level) → dungeon. Generate both downstream levels.
        var entranceRoom = surfaceLevel.Rooms.Single().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var entranceLevel = entranceRoom.Level;
        var entranceExitRoom = entranceLevel.Rooms.FirstOrDefault(r => (r.Features & RoomFeatures.Exit) != 0);
        Assert.NotNull(entranceExitRoom);
        var dungeonRoom = entranceExitRoom.Connections.Values
            .Select(c => c.Connection!.TargetRoomEntity.Room!)
            .FirstOrDefault(r => r.Level.Branch.Name == "dungeon");
        Assert.NotNull(dungeonRoom);
        LevelGenerator.EnsureGenerated(dungeonRoom);

        // After dungeon:1 generation, every '>' glyph from any dungeon:1 room targets the
        // SAME dungeon GameBranch and the SAME dungeon:2 GameLevel — never separate
        // duplicate instances per glyph.
        var dungeonBranches = game.Branches.Where(b => b.Name == "dungeon").ToList();
        Assert.Single(dungeonBranches);

        var dungeonDepth2Levels = dungeonBranches[0].Levels.Where(l => l.Depth == 2).ToList();
        Assert.Single(dungeonDepth2Levels);

        // All placeholder rooms (Width == 0) on dungeon:2 belong to that one level — and
        // there's exactly one placeholder regardless of how many '>' glyphs were processed.
        var depth2Rooms = dungeonDepth2Levels[0].Rooms;
        Assert.Single(depth2Rooms);
        Assert.Equal(0, depth2Rooms.First().Width);
    }

    /// <summary>
    ///     <see cref="LevelConnection.TryCreateReceivingConnection"/> reports no match (and wires
    ///     nothing) for a room with no dangling incoming, leaving the caller to source instead.
    /// </summary>
    [Fact]
    public void TryCreateReceivingConnection_returns_false_when_no_dangling_incoming()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var sourceLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var room = LevelLayout.CreateEmptyRoom(sourceLevel, manager);

        var received = LevelConnection.TryCreateReceivingConnection(
            room, new Point(0, 0), new LevelConnection { BranchName = "dungeon", Depth = 2 });

        Assert.False(received);
        Assert.Empty(room.Connections);
    }

    /// <summary>
    ///     <see cref="LevelConnection.TryCreateReceivingConnection"/> wires a still-dangling
    ///     incoming connection (the entrance case) and returns <c>true</c>, so the caller does not
    ///     source a new forward connection.
    /// </summary>
    [Fact]
    public void TryCreateReceivingConnection_wires_a_dangling_incoming()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);

        // The exit room sources a connection to a fresh target level, creating a dangling
        // incoming on that level's entrance placeholder.
        var sourceLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var exitRoom = LevelLayout.CreateEmptyRoom(sourceLevel, manager);
        exitRoom.Features = RoomFeatures.Exit;
        var down = LevelConnection.CreateSourceConnection(
            exitRoom, new Point(3, 0), new LevelConnection { BranchName = "dungeon", Depth = 2 });

        var entrancePlaceholder = down.TargetRoomEntity.Room!;
        Assert.Single(entrancePlaceholder.IncomingConnections);
        Assert.Null(down.TargetCell);

        var received = LevelConnection.TryCreateReceivingConnection(
            entrancePlaceholder, new Point(2, 2), connectionDefinition: null);

        Assert.True(received);
        Assert.Equal(new Point(2, 2), down.TargetCell);
    }

    /// <summary>
    ///     Two source connections from the same source level to the same target level must converge
    ///     on a single room — the target level's entrance. (e.g. the exit room and a non-exit static
    ///     connecting fragment both sourcing to the level below.)
    /// </summary>
    [Fact]
    public void Source_connections_to_one_target_level_converge_on_its_entrance_room()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);
        var sourceLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);

        var exitRoom = LevelLayout.CreateEmptyRoom(sourceLevel, manager);
        exitRoom.Features = RoomFeatures.Exit;
        var otherRoom = LevelLayout.CreateEmptyRoom(sourceLevel, manager);

        var def = new LevelConnection { BranchName = "dungeon", Depth = 2 };

        // The exit room sources first, creating the target level + its entrance placeholder.
        var exitConnection = LevelConnection.CreateSourceConnection(exitRoom, new Point(0, 0), def);
        // A second connection from another room to the same level converges on that entrance.
        var otherConnection = LevelConnection.CreateSourceConnection(otherRoom, new Point(1, 0), def);

        Assert.Equal(exitConnection.TargetRoomId, otherConnection.TargetRoomId);
        Assert.True((otherConnection.TargetRoomEntity.Room!.Features & RoomFeatures.Entrance) != 0,
            "Both connections should converge on the target level's entrance room.");
    }

    /// <summary>
    ///     When the target level is already materialized with more than one room, a source
    ///     connection to it must still target the entrance room (the room the exit connection points
    ///     to), not an arbitrary first room.
    /// </summary>
    [Fact]
    public void Source_connection_to_an_existing_level_targets_its_entrance_room()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;
        var dungeon = TestHelper.InstantiateBranch(BranchData.Dungeon, game);

        var sourceLevel = LevelGenerator.CreateEmpty(dungeon, depth: 1, seed: 1u, manager);
        var sourceRoom = LevelLayout.CreateEmptyRoom(sourceLevel, manager);

        // Pre-materialize the target level with a non-entrance room plus the entrance.
        var targetLevel = LevelGenerator.CreateEmpty(dungeon, depth: 2, seed: 1u, manager);
        LevelLayout.CreateEmptyRoom(targetLevel, manager);
        var entrance = LevelLayout.CreateEmptyRoom(targetLevel, manager);
        entrance.Features = RoomFeatures.Entrance;

        var connection = LevelConnection.CreateSourceConnection(
            sourceRoom, new Point(0, 0), new LevelConnection { BranchName = "dungeon", Depth = 2 });

        Assert.Equal(entrance.EntityId, connection.TargetRoomId);
    }

    /// <summary>
    ///     A <see cref="RoomFeatures.Shortcut"/> room sources a connection that skips one level
    ///     ahead (Depth + 2), versus <see cref="RoomFeatures.Exit"/>'s Depth + 1.
    /// </summary>
    [Fact]
    public void Shortcut_room_sources_a_connection_two_levels_ahead()
    {
        var (_, entity, game) = CreateShortcutBranch(length: 5);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, game.Manager);
        var room = LevelLayout.CreateEmptyRoom(level, game.Manager);
        room.Features = RoomFeatures.Shortcut;

        var connection = LevelConnection.CreateSourceConnection(room, new Point(2, 2), connectionDefinition: null);

        var target = connection.TargetRoomEntity.Room!;
        Assert.Equal("test-shortcut-branch", target.Level.BranchName);
        Assert.Equal(3, target.Level.Depth); // depth 1 + 2 (skip one level)
        Assert.Equal(Direction.Down, connection.Orientation);
    }

    [Fact]
    public void Exit_room_sources_one_level_ahead()
    {
        var (_, entity, game) = CreateShortcutBranch(length: 5);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, game.Manager);
        var room = LevelLayout.CreateEmptyRoom(level, game.Manager);
        room.Features = RoomFeatures.Exit;

        var connection = LevelConnection.CreateSourceConnection(room, new Point(2, 2), connectionDefinition: null);

        Assert.Equal(2, connection.TargetRoomEntity.Room!.Level.Depth); // depth 1 + 1
    }

    [Fact]
    public void Shortcut_past_the_end_of_the_branch_skips_into_the_next_branch()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.Branches.IncludeProduction = false;

        var originBranch = new Branch
        {
            Name = "test-origin", Length = 2, Difficulty = 1, Layout = new EmptyLevelLayout()
        };
        var nextBranch = new Branch
        {
            Name = "test-next", Length = 3, Difficulty = 1, Layout = new EmptyLevelLayout()
        };
        testProvider.Branches.Add(originBranch);
        testProvider.Branches.Add(nextBranch);

        var originEntity = originBranch.Instantiate(game);
        nextBranch.Instantiate(game);

        // Depth 2 in a length-2 branch: depth + 2 = 4 runs past the end → next branch.
        var level = LevelGenerator.CreateEmpty(originEntity, depth: 2, seed: 1u, game.Manager);
        var room = LevelLayout.CreateEmptyRoom(level, game.Manager);
        room.Features = RoomFeatures.Shortcut;

        var connection = LevelConnection.CreateSourceConnection(room, new Point(2, 2), connectionDefinition: null);

        Assert.Equal("test-next", connection.TargetRoomEntity.Room!.Level.BranchName);
    }

    /// <summary>
    ///     The layout connection gate (<see cref="RoomLayout.NeedsReceivingConnection"/> ||
    ///     <see cref="RoomLayout.NeedsSourceConnection"/>) is true for exit and shortcut rooms.
    /// </summary>
    [Fact]
    public void Exit_and_shortcut_rooms_need_a_source_connection()
    {
        var (_, entity, game) = CreateShortcutBranch(length: 5);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, game.Manager);

        var shortcutRoom = LevelLayout.CreateEmptyRoom(level, game.Manager);
        shortcutRoom.Features = RoomFeatures.Shortcut;
        Assert.True(NeedsConnectionProbe.Probe(shortcutRoom));

        var exitRoom = LevelLayout.CreateEmptyRoom(level, game.Manager);
        exitRoom.Features = RoomFeatures.Exit;
        Assert.True(NeedsConnectionProbe.Probe(exitRoom));

        var plainRoom = LevelLayout.CreateEmptyRoom(level, game.Manager);
        plainRoom.Features = RoomFeatures.Normal;
        Assert.False(NeedsConnectionProbe.Probe(plainRoom));
    }

    [Fact]
    public void Shortcut_room_in_a_terminal_branch_with_no_room_ahead_needs_no_connection()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var branch = new Branch
        {
            Name = "test-terminal", Length = 1, Difficulty = 1, IsTerminal = true, Layout = new EmptyLevelLayout()
        };
        ((TestDataProvider)game.Services.Data).Branches.Add(branch);
        var entity = branch.Instantiate(game);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, game.Manager);

        // depth 1 + 2 = 3 > length 1 and the branch is terminal → nowhere to skip to.
        var shortcutRoom = LevelLayout.CreateEmptyRoom(level, game.Manager);
        shortcutRoom.Features = RoomFeatures.Shortcut;
        Assert.False(NeedsConnectionProbe.Probe(shortcutRoom));
    }

    [Fact]
    public void GetConnectingRoleTag_returns_the_matching_role_tag()
    {
        var (_, entity, game) = CreateShortcutBranch(length: 5);
        var level = LevelGenerator.CreateEmpty(entity, depth: 1, seed: 1u, game.Manager);

        var plain = LevelLayout.CreateEmptyRoom(level, game.Manager);
        plain.Features = RoomFeatures.Normal;
        Assert.Null(RoomLayout.GetConnectingRoleTag(plain));

        var exit = LevelLayout.CreateEmptyRoom(level, game.Manager);
        exit.Features = RoomFeatures.Exit;
        Assert.Equal(MapFragment.FeatureTagNames[RoomFeatures.Exit], RoomLayout.GetConnectingRoleTag(exit));

        var shortcut = LevelLayout.CreateEmptyRoom(level, game.Manager);
        shortcut.Features = RoomFeatures.Shortcut;
        Assert.Equal(MapFragment.FeatureTagNames[RoomFeatures.Shortcut], RoomLayout.GetConnectingRoleTag(shortcut));
    }

    [Fact]
    public void Connecting_fragments_carry_no_role_tag()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var connecting = game.Services.Data.ConnectingMapFragments.GetAsList();

        // The only connecting fragment (and its transform variants) is RandomRectangle. It
        // carries no role tag (entrance / exit / shortcut) — those tags would only bias weight,
        // and an untagged fragment stays selectable for every connection role.
        Assert.NotEmpty(connecting);
        Assert.All(connecting, f =>
        {
            Assert.Equal("randomRectangle", f.SourceName);
            Assert.DoesNotContain(MapFragment.FeatureTagNames[RoomFeatures.Entrance], f.Tags!);
            Assert.DoesNotContain(MapFragment.FeatureTagNames[RoomFeatures.Exit], f.Tags!);
            Assert.DoesNotContain(MapFragment.FeatureTagNames[RoomFeatures.Shortcut], f.Tags!);
        });
    }

    private static (Branch Branch, GameBranch Entity, Game Game) CreateShortcutBranch(byte length)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var branch = new Branch
        {
            Name = "test-shortcut-branch",
            Length = length,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        ((TestDataProvider)game.Services.Data).Branches.Add(branch);
        return (branch, branch.Instantiate(game), game);
    }

    /// <summary>
    ///     Exposes the protected <see cref="RoomLayout.NeedsReceivingConnection"/> /
    ///     <see cref="RoomLayout.NeedsSourceConnection"/> gate (the inlined <c>NeedsLevelConnection</c>).
    /// </summary>
    private sealed class NeedsConnectionProbe : RoomLayout
    {
        public override List<Chunk>? Fill(
            RoomComponent room, Branch branch, DefiningMapFragment fragment,
            IReadOnlyList<RoomDoorwayAnchor> doorwayDemand, RoomParameters roomParameters)
            => throw new NotSupportedException();

        public static bool Probe(RoomComponent room)
            => NeedsReceivingConnection(room) || NeedsSourceConnection(room);
    }
}
