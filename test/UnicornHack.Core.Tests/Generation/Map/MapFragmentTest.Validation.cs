namespace UnicornHack.Generation.Map;

/// <summary>
///     Fail-fast validation tests for the fragment / room-parameter authoring
///     contracts. Each test exercises one validation gate documented in
///     <see cref="MapFragment" /> / <see cref="RoomParameters" /> /
///     <see cref="DefiningMapFragment" />; the production data files must satisfy
///     every gate or the failure surfaces at load (or first placement) instead of
///     as a mysterious mid-generation retry exhaustion.
/// </summary>
public partial class MapFragmentTest
{
    [Fact]
    public void RoomParameters_rejects_zero_width()
    {
        var ex = Assert.Throws<ArgumentException>(() => new RoomParameters(
            RoomWidth: 0, RoomHeight: 5, MapFeature.Default,
            new CreatureGenerator(), new ItemGenerator()));
        Assert.Contains("positive width/height", ex.Message);
    }

    [Fact]
    public void RoomParameters_rejects_zero_height()
    {
        Assert.Throws<ArgumentException>(() => new RoomParameters(
            RoomWidth: 5, RoomHeight: 0, MapFeature.Default,
            new CreatureGenerator(), new ItemGenerator()));
    }

    [Fact]
    public void RoomParameters_rejects_null_creature_generator()
    {
        Assert.Throws<ArgumentNullException>(() => new RoomParameters(
            RoomWidth: 5, RoomHeight: 5, MapFeature.Default,
            CreatureGenerator: null!, new ItemGenerator()));
    }

    [Fact]
    public void RoomParameters_rejects_null_item_generator()
    {
        Assert.Throws<ArgumentNullException>(() => new RoomParameters(
            RoomWidth: 5, RoomHeight: 5, MapFeature.Default,
            new CreatureGenerator(), ItemGenerator: null!));
    }

    [Fact]
    public void ValidatePlacementCapability_rejects_static_fragment_with_no_doorways_and_no_open_perimeter()
    {
        var fragment = new NormalMapFragment
        {
            Name = "no-doorways-fully-closed",
            Map = """
                #####
                #...#
                #...#
                #...#
                #####
                """,
            NoRandomDoorways = true,
            NoTransform = true
        };
        fragment.EnsureInitialized();

        // Pre-condition: this fragment is exactly the shape the validation should reject.
        Assert.Equal(0, fragment.PossibleDoorwayCount);
        Assert.False(fragment.HasOpenPerimeter);

        var ex = Assert.Throws<InvalidOperationException>(() => fragment.ValidatePlacementCapability());
        Assert.Contains("never connect", ex.Message);
    }

    [Fact]
    public void ValidatePlacementCapability_accepts_fragment_with_authored_doorways()
    {
        var fragment = new NormalMapFragment
        {
            Name = "authored-doorway",
            Map = """
                #####
                #...#
                +...+
                #...#
                #####
                """,
            NoTransform = true
        };
        fragment.EnsureInitialized();

        Assert.True(fragment.PossibleDoorwayCount > 0);
        fragment.ValidatePlacementCapability();
    }

    [Fact]
    public void ValidatePlacementCapability_accepts_open_perimeter_fragment_with_no_doorways()
    {
        var fragment = new NormalMapFragment
        {
            Name = "open-perimeter",
            Map = """
                ~FFFF
                F....
                F....
                F....
                F....
                """,
            NoRandomDoorways = true,
            NoTransform = true
        };
        fragment.EnsureInitialized();

        Assert.True(fragment.HasOpenPerimeter);
        fragment.ValidatePlacementCapability();
    }

    [Fact]
    public void ValidatePlacementCapability_skips_defining_fragments()
    {
        // Defining fragments are placed as the room's first / only chunk; doorway
        // demand is satisfied by surrounding fragments. The capability check is
        // therefore a no-op for them even when the static map carries no doorways.
        var fragment = new DefiningMapFragment
        {
            Name = "defining-no-doorway",
            Map = """
                #####
                #...#
                #...#
                #...#
                #####
                """,
            NoRandomDoorways = true,
            NoTransform = true
        };
        fragment.EnsureInitialized();

        Assert.Equal(0, fragment.PossibleDoorwayCount);
        Assert.False(fragment.HasOpenPerimeter);
        // Should not throw.
        fragment.ValidatePlacementCapability();
    }

    [Fact]
    public void Defining_fragment_placement_claims_its_cells_in_ChunkCells()
    {
        // Contract: a defining fragment with a static map produces a chunk just like
        // any other fragment, and that chunk claims every authored cell (inside +
        // perimeter) in RoomParameters.ChunkCells. The placement invariant is that
        // only the room's default terrain is allowed outside of a chunk — the
        // defining fragment is the first chunk and establishes its own non-default
        // payload, so its cells must be tracked as owned content.
        var game = TestHelper.CreateGame(1u);
        var fragment = new TestDefiningMapFragment
        {
            Name = "defining-claims-cells",
            Map = """
                #####
                #...#
                #...#
                #...#
                #####
                """,
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        var (room, _) = TestHelper.BuildRoom(fragment, game);
        var branch = room.Level.Game.Services.Data.Branches.Get(room.Level.BranchName);
        using var roomParameters = fragment.GetRoomParameters(branch);

        var ctx = new LotPlacementContext(room.BoundingRectangle, roomParameters);

        // Reset terrain + ownership so we can verify the placement writes them back.
        Array.Clear(room.Terrain, 0, room.TileCount);
        roomParameters.ClearChunkCells();
        var chunk = fragment.TryPlace(room, ctx);

        Assert.NotNull(chunk);
        Assert.Same(fragment, chunk!.Fragment);

        // The defining-fragment chunk claims every cell it authored.
        var claimed = 0;
        for (var y = 0; y < roomParameters.RoomHeight; y++)
        {
            for (var x = 0; x < roomParameters.RoomWidth; x++)
            {
                if (roomParameters[x, y] is not null)
                {
                    claimed++;
                }
            }
        }
        Assert.Equal(chunk.InsidePoints.Count + chunk.PerimeterPoints.Count, claimed);
        foreach (var p in chunk.InsidePoints)
        {
            Assert.Same(chunk, roomParameters[p]);
        }
        foreach (var p in chunk.PerimeterPoints)
        {
            Assert.Same(chunk, roomParameters[p]);
        }
    }

    [Fact]
    public void Cells_outside_any_chunk_carry_only_the_default_terrain()
    {
        // Companion to the above: post-placement, every cell whose terrain differs
        // from RoomParameters.DefaultTerrain must be claimed by some chunk in
        // ChunkCells. This is the invariant the walkable-wins overlap rule depends
        // on (see MapFragment.ShouldPreserveAgainstWallWrite).
        var game = TestHelper.CreateGame(1u);
        var fragment = new TestDefiningMapFragment
        {
            Name = "defining-invariant",
            Map = """
                #####
                #...#
                #...#
                #...#
                #####
                """,
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        var (room, _) = TestHelper.BuildRoom(fragment, game);
        var branch = room.Level.Game.Services.Data.Branches.Get(room.Level.BranchName);
        using var roomParameters = fragment.GetRoomParameters(branch);
        var ctx = new LotPlacementContext(room.BoundingRectangle, roomParameters);

        Array.Clear(room.Terrain, 0, room.TileCount);
        roomParameters.ClearChunkCells();
        Assert.NotNull(fragment.TryPlace(room, ctx));

        var pointToIndex = room.PointToIndex!;
        for (byte y = 0; y < room.Height; y++)
        {
            for (byte x = 0; x < room.Width; x++)
            {
                var p = new Point(x, y);
                var terrain = room.Terrain[pointToIndex[x, y]];
                if (terrain == roomParameters.DefaultTerrain)
                {
                    continue;
                }
                Assert.True(roomParameters[x, y] is not null,
                    $"Cell {p} carries non-default terrain {terrain} but is not in ChunkCells.");
            }
        }
    }

    [Fact]
    public void Feature_glyph_in_conditional_edge_row_throws()
    {
        // Conditional edge rows ('~'-bracketed) accept only the edge-condition vocabulary.
        // A feature glyph there is meaningless and the parser must reject it loudly.
        var fragment = new NormalMapFragment
        {
            Name = "bad-edge-glyph",
            Map = """
                ~___~
                #####
                #...#
                #####
                """
        };

        var ex = Assert.Throws<InvalidOperationException>(() => fragment.EnsureInitialized());
        Assert.Contains("invalid edge condition symbol", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Invalid_edge_symbol_throws()
    {
        var ex = AssertDefiningInitThrows(@"
~Q...~
......
~....~");

        Assert.Contains("invalid edge condition symbol 'Q'", ex.Message);
    }

    [Fact]
    public void Empty_substitution_is_rejected_at_load()
    {
        var fragment = new NormalMapFragment
        {
            Name = "annotation-empty",
            Map = """
                ###
                #1#
                ###
                """,
            Annotations = new Dictionary<char, string> { ['1'] = string.Empty },
            NoRandomDoorways = true,
            NoTransform = true
        };

        var ex = Assert.Throws<InvalidOperationException>(() => fragment.EnsureInitialized());
        Assert.Contains("annotation '1' has an empty substitution string", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     When the static map has no walls at all (so the perimeter-scan finds nothing)
    ///     and <see cref="MapFragment.NoRandomDoorways" /> is false, placement must throw
    ///     instead of producing an unreachable chunk.
    /// </summary>
    [Fact]
    public void Throws_when_no_doorway_candidates_can_be_derived()
    {
        var (room, lot, rp) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = new NormalMapFragment
        {
            Name = "all-walls",
            Map = """
                ###
                ###
                ###
                """
        };
        fragment.EnsureInitialized();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            fragment.TryPlace(room, new LotPlacementContext(lot, rp));
        });
        // TryPlace wraps the underlying error; assert the inner message mentions the cause.
        Assert.Contains("no doorway candidates", ex.InnerException?.Message ?? ex.Message);
    }

    private static InvalidOperationException AssertDefiningInitThrows(string map)
    {
        _ = TestHelper.CreateGame(1u);
        var fragment = NewDefiningFragment(map);
        return Assert.Throws<InvalidOperationException>(() => fragment.EnsureInitialized());
    }
}
