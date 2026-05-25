namespace UnicornHack.Generation.Map;

public partial class MapFragmentTest
{
    [Fact]
    public void All_four_corners_marked_makes_all_edges_conditional()
    {
        var fragment = InitializeDefining(@"
~....~
......
......
~....~");

        Assert.True(fragment.ConditionalFirstRow);
        Assert.True(fragment.ConditionalLastRow);
        Assert.True(fragment.ConditionalFirstColumn);
        Assert.True(fragment.ConditionalLastColumn);

        Assert.NotNull(fragment.NorthEdge);
        Assert.NotNull(fragment.SouthEdge);
        Assert.NotNull(fragment.WestEdge);
        Assert.NotNull(fragment.EastEdge);

        Assert.Equal(fragment.PayloadArea.Width, fragment.NorthEdge.Length);
        Assert.Equal(fragment.PayloadArea.Width, fragment.SouthEdge.Length);
        Assert.Equal(fragment.PayloadArea.Height, fragment.WestEdge.Length);
        Assert.Equal(fragment.PayloadArea.Height, fragment.EastEdge.Length);

        var stoneFloor = EdgeConditionKind.StoneFloor;
        Assert.All(fragment.NorthEdge, e => Assert.Equal(stoneFloor, e));
        Assert.All(fragment.SouthEdge, e => Assert.Equal(stoneFloor, e));
        Assert.All(fragment.WestEdge, e => Assert.Equal(stoneFloor, e));
        Assert.All(fragment.EastEdge, e => Assert.Equal(stoneFloor, e));
    }

    [Fact]
    public void No_conditional_border_leaves_edge_arrays_null()
    {
        var fragment = InitializeDefining(@"
....
....");

        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalLastRow);
        Assert.False(fragment.ConditionalFirstColumn);
        Assert.False(fragment.ConditionalLastColumn);

        Assert.Null(fragment.NorthEdge);
        Assert.Null(fragment.SouthEdge);
        Assert.Null(fragment.WestEdge);
        Assert.Null(fragment.EastEdge);
    }

    [Fact]
    public void North_edge_parses_symbol_kinds()
    {
        var fragment = InitializeDefining(@"
~SFUE~
......
~....~");

        Assert.NotNull(fragment.NorthEdge);
        Assert.Equal(4, fragment.NorthEdge.Length);
        // 'S' resolves at parse time to the tile-specific kind matching the adjacent
        // payload tile (here a '.' StoneFloor on row 1, column 1).
        Assert.Equal(EdgeConditionKind.StoneFloor, fragment.NorthEdge[0]);
        Assert.Equal(EdgeConditionKind.Filled, fragment.NorthEdge[1]);
        Assert.Equal(EdgeConditionKind.Unfilled, fragment.NorthEdge[2]);
        Assert.Equal(EdgeConditionKind.Edge, fragment.NorthEdge[3]);
    }

    [Fact]
    public void Edge_tile_glyphs_become_tile_specific()
    {
        var fragment = InitializeDefining(@"
~#,A.~
......
~....~");

        Assert.NotNull(fragment.NorthEdge);
        Assert.Equal(EdgeConditionKind.StoneWall, fragment.NorthEdge[0]);
        Assert.Equal(EdgeConditionKind.RockFloor, fragment.NorthEdge[1]);
        Assert.Equal(EdgeConditionKind.StoneArchway, fragment.NorthEdge[2]);
        Assert.Equal(EdgeConditionKind.StoneFloor, fragment.NorthEdge[3]);
    }

    [Fact]
    public void Edge_plus_glyph_matches_floor_or_archway()
    {
        var fragment = InitializeDefining(@"
~.+.~
.....
~...~");

        Assert.NotNull(fragment.NorthEdge);
        Assert.Equal(3, fragment.NorthEdge.Length);
        Assert.Equal(EdgeConditionKind.StoneFloor, fragment.NorthEdge[0]);
        Assert.Equal(EdgeConditionKind.Doorway, fragment.NorthEdge[1]);
        Assert.Equal(EdgeConditionKind.StoneFloor, fragment.NorthEdge[2]);
        // '+' on a conditional edge row is an edge symbol, not an authored doorway candidate.
        Assert.Equal(0, fragment.PossibleDoorwayCount);
    }

    [Fact]
    public void Both_right_corners_mark_only_the_east_edge()
    {
        // Top-right and bottom-right are '~'. Per the both-corners rule, only the
        // east edge (which shares both corners) is conditional; the north and south
        // edges each have only one '~' corner and remain non-conditional.
        var fragment = InitializeDefining(@"
....~
.....
....~");

        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalLastRow);
        Assert.False(fragment.ConditionalFirstColumn);
        Assert.True(fragment.ConditionalLastColumn);

        Assert.Null(fragment.NorthEdge);
        Assert.Null(fragment.SouthEdge);
        Assert.Null(fragment.WestEdge);
        Assert.NotNull(fragment.EastEdge);
    }

    [Fact]
    public void Vertical_edge_parses_per_row()
    {
        // Top-left and bottom-left '~' mark the west edge as conditional; without
        // matching corners on the top-right / bottom-right the north and south edges
        // remain non-conditional.
        var fragment = InitializeDefining(@"
~....
S....
F....
U....
~....");

        Assert.True(fragment.ConditionalFirstColumn);
        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalLastRow);
        Assert.False(fragment.ConditionalLastColumn);

        Assert.NotNull(fragment.WestEdge);
        Assert.Equal(fragment.PayloadArea.Height, fragment.WestEdge.Length);
        Assert.Equal(5, fragment.WestEdge.Length);
        // The two corner rows carry the '~' marker and contribute Default slots; the
        // three middle rows carry S/F/U which resolve to tile-specific kinds.
        Assert.Equal(EdgeConditionKind.Default, fragment.WestEdge[0]);
        // 'S' resolves at parse time to the tile-specific kind matching the adjacent
        // payload tile (here a '.' StoneFloor on column 1, row 1).
        Assert.Equal(EdgeConditionKind.StoneFloor, fragment.WestEdge[1]);
        Assert.Equal(EdgeConditionKind.Filled, fragment.WestEdge[2]);
        Assert.Equal(EdgeConditionKind.Unfilled, fragment.WestEdge[3]);
        Assert.Equal(EdgeConditionKind.Default, fragment.WestEdge[4]);
    }

    [Fact]
    public void Tilde_inside_payload_becomes_pool()
    {
        var fragment = InitializeDefining(@"
......
.~....
......");

        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalLastRow);
        Assert.False(fragment.ConditionalFirstColumn);
        Assert.False(fragment.ConditionalLastColumn);
        Assert.Equal((byte)'~', fragment.ByteMap![fragment.PointToIndex![1, 1]]);
    }

    [Fact]
    public void Tilde_on_non_corner_edge_is_accepted_when_no_conditional_row()
    {
        var fragment = InitializeDefining(@"
.~....
......
......");

        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalLastRow);
        Assert.Equal((byte)'~', fragment.ByteMap![fragment.PointToIndex![1, 0]]);
    }

    [Fact]
    public void Single_corner_does_not_mark_any_edge()
    {
        // Only the top-left corner is '~'. Neither the north nor the west edge has
        // both corners marked, so both stay non-conditional. The lone '~' becomes a
        // payload Pool tile at the corner.
        var fragment = InitializeDefining(@"
~....
.....
.....");

        Assert.False(fragment.ConditionalFirstRow);
        Assert.False(fragment.ConditionalFirstColumn);
        Assert.False(fragment.ConditionalLastRow);
        Assert.False(fragment.ConditionalLastColumn);

        Assert.Null(fragment.NorthEdge);
        Assert.Null(fragment.WestEdge);
        Assert.Null(fragment.SouthEdge);
        Assert.Null(fragment.EastEdge);
    }

    [Fact]
    public void GetEdge_returns_matching_array_for_cardinals()
    {
        var fragment = InitializeDefining(@"
~....~
......
~....~");

        Assert.Same(fragment.NorthEdge, fragment.GetEdge(Direction.North));
        Assert.Same(fragment.SouthEdge, fragment.GetEdge(Direction.South));
        Assert.Same(fragment.WestEdge, fragment.GetEdge(Direction.West));
        Assert.Same(fragment.EastEdge, fragment.GetEdge(Direction.East));
    }

    [Theory]
    [InlineData(Direction.Northeast)]
    [InlineData(Direction.Northwest)]
    [InlineData(Direction.Southeast)]
    [InlineData(Direction.Southwest)]
    [InlineData(Direction.Up)]
    [InlineData(Direction.Down)]
    public void GetEdge_throws_for_non_cardinal_directions(Direction direction)
    {
        var fragment = InitializeDefining(@"
~....~
......
~....~");

        Assert.Throws<ArgumentOutOfRangeException>(() => fragment.GetEdge(direction));
    }

    private static TestDefiningMapFragment InitializeDefining(string map)
    {
        _ = TestHelper.CreateGame(1u);
        var fragment = NewDefiningFragment(map);
        fragment.EnsureInitialized();
        return fragment;
    }

    private static TestDefiningMapFragment NewDefiningFragment(string map)
        => new()
        {
            Map = map,
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };
}
