using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Exercises <see cref="MapFragment.TryPlaceStaticMap" />'s perimeter-wall doorway
///     derivation (the path triggered when a static map has no explicit '?' markers and
///     <see cref="MapFragment.NoRandomDoorways" /> is false).
/// </summary>
public class StaticMapDoorwayTest
{
    /// <summary>
    ///     A 3x3 walled box with a floor centre, placed inside a stone-floor room with
    ///     space on every side, should yield doorway candidates at the four wall midpoints
    ///     (corners are excluded).
    /// </summary>
    [Fact]
    public void Derives_doorways_at_four_midpoints_when_surrounded_by_floor()
    {
        var (room, lot) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, lot);

        Assert.NotNull(chunk);
        Assert.Equal(4, chunk!.DoorwayPoints.Count);

        var expected = new[]
        {
            new Point(2, 1), // top midpoint
            new Point(1, 2), // left midpoint
            new Point(3, 2), // right midpoint
            new Point(2, 3) // bottom midpoint
        };
        TestAssert.Equal(expected.OrderBy(p => p.Y).ThenBy(p => p.X),
            chunk.DoorwayPoints.OrderBy(p => p.Y).ThenBy(p => p.X), "");
    }

    /// <summary>
    ///     Corner perimeter tiles must never be picked: a corner has no single inward
    ///     direction. A 3x3 box surrounded by floor should never list any corner.
    /// </summary>
    [Fact]
    public void Skips_corner_perimeter_tiles()
    {
        var (room, lot) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, lot);

        Assert.NotNull(chunk);
        Assert.DoesNotContain(new Point(1, 1), chunk!.DoorwayPoints);
        Assert.DoesNotContain(new Point(3, 1), chunk.DoorwayPoints);
        Assert.DoesNotContain(new Point(1, 3), chunk.DoorwayPoints);
        Assert.DoesNotContain(new Point(3, 3), chunk.DoorwayPoints);
    }

    /// <summary>
    ///     When two of the box's walls touch the room boundary, those two sides have no
    ///     valid corridor outside the chunk and must be excluded — only the two interior
    ///     sides should yield doorway candidates.
    /// </summary>
    [Fact]
    public void Excludes_sides_touching_room_boundary()
    {
        // 4x4 room; place the 3x3 fragment in the top-left corner so its top and left
        // walls coincide with the room boundary.
        var (room, lot) = BuildSurroundedRoom(roomSize: 4, lotTopLeft: new Point(0, 0), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, lot);

        Assert.NotNull(chunk);
        // Only the bottom and right sides have outside floor tiles.
        var expected = new[]
        {
            new Point(2, 1), // right midpoint  — outside is (3,1) which is floor
            new Point(1, 2) // bottom midpoint — outside is (1,3) which is floor
        };
        TestAssert.Equal(expected.OrderBy(p => p.Y).ThenBy(p => p.X),
            chunk!.DoorwayPoints.OrderBy(p => p.Y).ThenBy(p => p.X), "");
    }

    /// <summary>
    ///     When the static map has no walls at all (so the perimeter-scan finds nothing)
    ///     and <see cref="MapFragment.NoRandomDoorways" /> is false, placement must throw
    ///     instead of producing an unreachable chunk.
    /// </summary>
    [Fact]
    public void Throws_when_no_doorway_candidates_can_be_derived()
    {
        var (room, lot) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = new NormalMapFragment
        {
            Name = "all-walls",
            Map = """
                ###
                ###
                ###
                """
        };
        fragment.EnsureInitialized(room.Game);

        var ex = Assert.Throws<InvalidOperationException>(() => fragment.TryPlace(room, lot));
        // TryPlace wraps the underlying error; assert the inner message mentions the cause.
        Assert.Contains("no doorway candidates", ex.InnerException?.Message ?? ex.Message);
    }

    /// <summary>
    ///     <see cref="MapFragment.NoRandomDoorways" /> opts the fragment out entirely —
    ///     no derivation, no throw, just an empty doorway list.
    /// </summary>
    [Fact]
    public void NoRandomDoorways_skips_derivation()
    {
        var (room, lot) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = new NormalMapFragment
        {
            Name = "opted-out",
            Map = """
                ###
                #.#
                ###
                """,
            NoRandomDoorways = true
        };
        fragment.EnsureInitialized(room.Game);

        var chunk = fragment.TryPlace(room, lot);

        Assert.NotNull(chunk);
        Assert.Empty(chunk!.DoorwayPoints);
    }

    /// <summary>
    ///     Builds a square room of <paramref name="roomSize" /> filled with stone floor
    ///     (via a defining fragment of '.'s with NoRandomDoorways=true) and returns the
    ///     room together with a bounding rectangle the caller can pass to
    ///     <see cref="MapFragment.TryPlace" /> to deterministically place a fragment of
    ///     the requested size at the requested top-left.
    /// </summary>
    private static (RoomComponent Room, Rectangle Lot) BuildSurroundedRoom(
        byte roomSize, Point lotTopLeft, byte lotSize)
    {
        // Build an N×N floor map: each row is N dots followed by '\n'.
        var rowLine = new string('.', roomSize);
        var rows = new string[roomSize];
        for (var i = 0; i < roomSize; i++)
        {
            rows[i] = rowLine;
        }

        var room = TestHelper.BuildRoom(string.Join('\n', rows));
        var lot = new Rectangle(lotTopLeft, lotSize, lotSize);
        return (room, lot);
    }

    /// <summary>
    ///     A 3x3 fragment: walls on the perimeter, single floor tile in the middle.
    /// </summary>
    private static NormalMapFragment BuildBoxFragment(Game game)
    {
        var fragment = new NormalMapFragment
        {
            Name = "3x3-box",
            Map = """
                ###
                #.#
                ###
                """
        };
        fragment.EnsureInitialized(game);
        return fragment;
    }
}
