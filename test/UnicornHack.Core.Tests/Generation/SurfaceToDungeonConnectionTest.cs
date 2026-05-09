using UnicornHack.Data.Branches;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

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
public class SurfaceToDungeonConnectionTest
{
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
}
