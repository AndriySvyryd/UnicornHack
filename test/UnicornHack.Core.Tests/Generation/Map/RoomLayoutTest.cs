using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Tests for <see cref="UniformRoomLayout" />. The trim must
///     drop corridor tiles that already lie inside the source or target chunk's interior,
///     so <see cref="UniformRoomLayout" />'s subsequent corridor write doesn't carve a
///     redundant archway through perimeter walls reached by a path that diverted through
///     the chunk's own walkable interior.
/// </summary>
public class RoomLayoutTest
{
    /// <summary>
    ///     Path lives entirely outside both chunks (the typical case) — trim is a no-op.
    /// </summary>
    [Fact]
    public void Returns_path_unchanged_when_no_tile_is_inside_either_chunk()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(6, 0), 3, 3), inside: [new Point(7, 1)]);

        // Corridor of three tiles between the chunks; none are inside either chunk.
        var path = new[] { new Point(3, 1), new Point(4, 1), new Point(5, 1) };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Same(path, trimmed);
    }

    /// <summary>
    ///     Path starts at the source chunk's connection point and walks through two
    ///     interior tiles before exiting toward the target. The trim must drop the first
    ///     three entries (the perimeter doorway and both interior tiles), leaving the
    ///     corridor to begin at the first tile past the chunk's interior.
    /// </summary>
    [Fact]
    public void Drops_source_chunk_prefix_through_the_last_inside_point()
    {
        var room = TestHelper.BuildRoom();
        var sourceInside = new[] { new Point(1, 1), new Point(2, 1) };
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 3), inside: sourceInside);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 3, 3), inside: [new Point(8, 1)]);

        // (0,1) doorway perimeter wall → two interior tiles → corridor → target side.
        var path = new[]
        {
            new Point(0, 1), // perimeter doorway of source
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source (last)
            new Point(3, 1), // first tile past source interior — kept
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        }, trimmed);
    }

    /// <summary>
    ///     Mirror of the source-side case: path approaches the target chunk and dives
    ///     into its interior before stopping at the target doorway. Trim drops everything
    ///     from the first interior tile (from the target end) onward.
    /// </summary>
    [Fact]
    public void Drops_target_chunk_suffix_from_the_first_inside_point()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var targetInside = new[] { new Point(7, 1), new Point(8, 1) };
        var target = BuildChunk(room, new Rectangle(new Point(6, 0), 4, 3), inside: targetInside);

        var path = new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1), // perimeter of target
            new Point(7, 1), // inside target (first from end)
            new Point(8, 1), // inside target
            new Point(9, 1)  // perimeter of target
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1)
        }, trimmed);
    }

    /// <summary>
    ///     Both ends dive into their respective chunk's interior. Trim drops both prefix
    ///     and suffix, leaving only the bridge tiles between the two chunks.
    /// </summary>
    [Fact]
    public void Drops_both_ends_when_path_enters_each_chunk_interior()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 3), inside: [new Point(1, 1), new Point(2, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 4, 3), inside: [new Point(8, 1), new Point(9, 1)]);

        var path = new[]
        {
            new Point(0, 1),
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1),
            new Point(7, 1),
            new Point(8, 1), // inside target
            new Point(9, 1), // inside target
            new Point(10, 1)
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(3, 1),
            new Point(4, 1),
            new Point(5, 1),
            new Point(6, 1),
            new Point(7, 1)
        }, trimmed);
    }

    /// <summary>
    ///     The corridor wanders out of and back into the source chunk before leaving for
    ///     good. Trim must keep walking until it finds the LAST inside point so the
    ///     intermediate excursion through chunk interior is dropped as well.
    /// </summary>
    [Fact]
    public void Trims_through_the_last_inside_point_when_path_re_enters_source_chunk()
    {
        var room = TestHelper.BuildRoom();
        var sourceInside = new[] { new Point(1, 1), new Point(2, 1), new Point(1, 3) };
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 4, 5), inside: sourceInside);
        var target = BuildChunk(room, new Rectangle(new Point(7, 0), 3, 3), inside: [new Point(8, 1)]);

        var path = new[]
        {
            new Point(0, 1),
            new Point(1, 1), // inside source
            new Point(2, 1), // inside source
            new Point(3, 1),
            new Point(2, 2), // outside source interior (not in inside list)
            new Point(1, 3), // inside source — LAST occurrence
            new Point(1, 4),
            new Point(2, 4),
            new Point(3, 4),
            new Point(4, 4),
            new Point(5, 4)
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(1, 4),
            new Point(2, 4),
            new Point(3, 4),
            new Point(4, 4),
            new Point(5, 4)
        }, trimmed);
    }

    /// <summary>
    ///     A 1-tile-wide gap between the two chunks: only the doorway perimeter wall on
    ///     each side survives the trim, leaving the corridor to carve just those two
    ///     tiles into archways.
    /// </summary>
    [Fact]
    public void Keeps_perimeter_doorways_when_chunks_are_adjacent()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(3, 0), 3, 3), inside: [new Point(4, 1)]);

        // (1,1) source interior → (2,1) source perimeter → (3,1) target perimeter →
        // (4,1) target interior. Trim drops the two interior tiles but must keep the
        // adjacent perimeter walls so WriteCorridor can carve the archways.
        var path = new[]
        {
            new Point(1, 1),
            new Point(2, 1),
            new Point(3, 1),
            new Point(4, 1)
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Equal(new[]
        {
            new Point(2, 1),
            new Point(3, 1)
        }, trimmed);
    }

    /// <summary>
    ///     When every tile lives in a chunk's interior the trim drops the entire path —
    ///     the chunks already cover the route end-to-end and no corridor write is needed.
    /// </summary>
    [Fact]
    public void Returns_empty_when_path_is_entirely_inside_chunk_interiors()
    {
        var room = TestHelper.BuildRoom();
        var source = BuildChunk(room, new Rectangle(new Point(0, 0), 3, 3), inside: [new Point(1, 1), new Point(2, 1)]);
        var target = BuildChunk(room, new Rectangle(new Point(3, 0), 3, 3), inside: [new Point(3, 1), new Point(4, 1)]);

        var path = new[]
        {
            new Point(1, 1),
            new Point(2, 1),
            new Point(3, 1),
            new Point(4, 1)
        };

        var trimmed = UniformRoomLayout.TrimCorridorEndpoints(path, source, target);

        Assert.Empty(trimmed);
    }

    /// <summary>
    ///     Sanity check on the new <see cref="Chunk.PerimeterPoints" /> collection: the
    ///     fragment-static path populates it with every wall, archway and explicit
    ///     doorway tile written by the fragment so corridor logic downstream can
    ///     distinguish chunk-owned tiles from open floor.
    /// </summary>
    [Fact]
    public void Static_map_chunk_exposes_perimeter_points_from_fragment()
    {
        var room = TestHelper.BuildRoom(new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5) + "\n"
                                        + new string('.', 5));
        var fragment = new NormalMapFragment
        {
            Name = "perimeter-box",
            Map = """
                  ###
                  #.#
                  ###
                  """
        };
        fragment.EnsureInitialized(room.Game);

        var chunk = fragment.TryPlace(room, new Rectangle(new Point(1, 1), 3, 3));

        Assert.NotNull(chunk);
        // The 3x3 box has 8 wall tiles around a single interior floor.
        Assert.Equal(8, chunk!.PerimeterPoints.Count);
        Assert.Single(chunk.InsidePoints);
        Assert.Contains(new Point(2, 2), chunk.InsidePoints);
    }

    private static Chunk BuildChunk(
        RoomComponent room,
        Rectangle bounds,
        IReadOnlyList<Point> inside,
        IReadOnlyList<Point>? doorways = null,
        IReadOnlyList<Point>? perimeter = null)
    {
        // A non-null MapFragment is required by the Chunk constructor but plays no role in
        // the trim algorithm — use a trivial fragment whose only purpose is to satisfy
        // the type system.
        var fragment = new NormalMapFragment { Name = "trim-test-stub", Map = "." };
        fragment.EnsureInitialized(room.Game);
        return new Chunk(room, bounds,
            doorwayPoints: doorways != null ? new HashSet<Point>(doorways) : null,
            insidePoints: new HashSet<Point>(inside),
            perimeterPoints: perimeter != null ? new HashSet<Point>(perimeter) : null,
            predefinedPoints: [],
            fragment: fragment);
    }
}
