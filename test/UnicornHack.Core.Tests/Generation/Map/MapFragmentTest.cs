using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public partial class MapFragmentTest
{
    /// <summary>
    ///     Builds a minimal <see cref="RoomParameters" /> suitable for unit tests that
    ///     drive placement primitives directly (no real defining-fragment metadata).
    /// </summary>
    internal static RoomParameters TestRoomParameters(MapFeature defaultTerrain = MapFeature.Default)
        => new(1, 1, defaultTerrain, new CreatureGenerator(), new ItemGenerator());

    /// <summary>
    ///     <see cref="RoomParameters" /> sized to <paramref name="room" />, suitable for
    ///     tests that build a real room (so <see cref="RoomParameters" />'s chunk-cell map
    ///     spans the room's coordinate space) but skip the full defining-fragment metadata.
    /// </summary>
    internal static RoomParameters TestRoomParameters(
        RoomComponent room, MapFeature defaultTerrain = MapFeature.Default)
        => new(
            room.Width == 0 ? (byte)1 : room.Width,
            room.Height == 0 ? (byte)1 : room.Height,
            defaultTerrain,
            new CreatureGenerator(),
            new ItemGenerator());

    [Fact]
    public void BuildRoom_Point() => BuildRoomTest(@"
O");

    [Fact]
    public void BuildRoom_Hairy_Point() => BuildRoomTest(@"
 O
OOO
 O");

    [Fact]
    public void BuildRoom_Small_Square() => BuildRoomTest(@"
PP
PP");

    [Fact]
    public void BuildRoom_Hairy_Small_Square() => BuildRoomTest(@"
OO O
 PPO
OPP
O OO");

    [Fact]
    public void BuildRoom_Rectangle() => BuildRoomTest(@"
PPP
PIP
PPP");

    [Fact]
    public void BuildRoom_Hairy_Rectangle() => BuildRoomTest(@"
 O O
OPPPO
 PIP
OPPPO
 O O");

    [Fact]
    public void BuildRoom_Hollow_Rectangle() => BuildRoomTest(@"
PPP OOO
PIPOO O
PPP OOO");

    [Fact]
    public void BuildRoom_I() => BuildRoomTest(@"
OOOOO
O O O
  O  X
O O O
OOOOO");

    [Fact]
    public void BuildRoom_Ring() => BuildRoomTest(@"
OOO
O O
OOPP
  PP");

    [Fact]
    public void BuildRoom_Thick_Rectangle() => BuildRoomTest(@"
PPPPPP
PXXXXP
PX  XP
PX  XP
PXXXXP
PPPPPP");

    [Fact]
    public void BuildRoom_Touching_Squares() => BuildRoomTest(@"
PP X
PP
 PP
 PP");

    private void BuildRoomTest(string map)
    {
        var game = TestHelper.CreateGame(1u);
        var fragment = new TestDefiningMapFragment
        {
            Map = map,
            // These fixtures intentionally exercise tiny / pathological shapes that have
            // no valid doorway candidates; opt out of the doorway-required guard.
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        fragment.EnsureInitialized();
        var byteMap = fragment.ByteMap;
        fragment.SetByteMap(new byte[0]);
        var (level, rooms) = TestHelper.BuildRoom(fragment, game);

        var input = new List<Point>();
        var expectedInside = new List<Point>();
        var expectedPerimeter = new List<Point>();

        fragment.SetByteMap(byteMap);
        fragment.WriteMap(new Point(0, 0),
            level,
            (c, point, _, s) =>
            {
                if (c != ' ')
                {
                    input.Add(point);
                }

                switch (c)
                {
                    case 'I':
                        s.expectedInside.Add(point);
                        break;
                    case 'P':
                        s.expectedPerimeter.Add(point);
                        break;
                }
            },
            (expectedInside, expectedPerimeter));

        var room = fragment.BuildChunk(level, input, minDoorways: 0, TestRoomParameters(level))!;

        try
        {
            Assert.Equal(fragment.Width, room.BoundingRectangle.Width);
            Assert.Equal(fragment.Height, room.BoundingRectangle.Height);
            TestAssert.Equal(expectedInside, room.InsidePoints, "");
            TestAssert.Equal(expectedPerimeter, room.PerimeterPoints, "");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Seed: " + level.Game.Options.InitialSeed, e);
        }
    }

    private static NormalMapFragment NewFeatureFragment(string map)
    {
        var f = new NormalMapFragment
        {
            Name = "feature-glyph-test",
            Map = map,
            NoRandomDoorways = true,
            NoTransform = true
        };
        f.EnsureInitialized();
        return f;
    }

    private static (RoomComponent Room, Rectangle Lot, RoomParameters Parameters) BuildFloorRoom(byte size)
    {
        var row = new string('.', size);
        var rows = new string[size];
        for (var i = 0; i < size; i++)
        {
            rows[i] = row;
        }

        var room = TestHelper.BuildRoom(string.Join('\n', rows));
        // The room's defining-fragment chunk claims its cells in its own
        // RoomParameters; the placements driven by these tests use a bare
        // LotPlacementContext with a fresh RoomParameters whose chunk-cell map
        // is empty — every cell looks unowned to the walkable-wins rule, so
        // the candidate fragment's wall payload writes through freely.
        return (room, new Rectangle(new Point(0, 0), size, size), TestRoomParameters(room));
    }

    [Fact]
    public void Records_concrete_features_at_payload_positions()
    {
        var fragment = NewFeatureFragment("""
            #####
            #_.{#
            #...#
            #####
            """);
        var (room, lot, rp) = BuildFloorRoom(7);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        Assert.NotNull(chunk);
        var origin = chunk!.BoundingRectangle.TopLeft;
        var features = chunk.FeaturePoints.ToDictionary(t => t.Point, t => t.Feature);
        Assert.Equal(2, features.Count);
        Assert.Equal(RoomFeatures.DeityAltar, features[new Point((byte)(origin.X + 1), (byte)(origin.Y + 1))]);
        Assert.Equal(RoomFeatures.SmallFountain, features[new Point((byte)(origin.X + 3), (byte)(origin.Y + 1))]);
    }

    [Fact]
    public void Wildcard_glyph_records_null_feature()
    {
        var fragment = NewFeatureFragment("""
            ###
            #$#
            ###
            """);
        var (room, lot, rp) = BuildFloorRoom(7);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        var wildcard = Assert.Single(chunk!.FeaturePoints);
        var origin = chunk.BoundingRectangle.TopLeft;
        Assert.Equal(new Point((byte)(origin.X + 1), (byte)(origin.Y + 1)), wildcard.Point);
        Assert.Null(wildcard.Feature);
    }

    [Fact]
    public void Creature_wildcard_records_position_separately()
    {
        var fragment = NewFeatureFragment("""
            ###
            #%#
            ###
            """);
        var (room, lot, rp) = BuildFloorRoom(7);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        Assert.Empty(chunk!.FeaturePoints);
        var origin = chunk.BoundingRectangle.TopLeft;
        Assert.Equal([new Point((byte)(origin.X + 1), (byte)(origin.Y + 1))], chunk.CreaturePoints);
    }

    [Fact]
    public void Feature_glyph_cells_count_as_inside_points()
    {
        var fragment = NewFeatureFragment("""
            ###
            #_#
            ###
            """);
        var (room, lot, rp) = BuildFloorRoom(7);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        var origin = chunk!.BoundingRectangle.TopLeft;
        Assert.Contains(new Point((byte)(origin.X + 1), (byte)(origin.Y + 1)), chunk.InsidePoints);
    }

    [Fact]
    public void Glyph_table_keys_do_not_collide_with_terrain_or_edge_vocabulary()
    {
        // Reserved by terrain parsing (., ,, #, A, ~), the doorway candidate glyph (+),
        // connection identifiers (<, >), the trap glyph (^), padding ( ), and edge-condition
        // parsing (S, F, U, E). Adding any of these to FeatureGlyphs.Keys would silently
        // shadow a built-in role. The creature wildcard '%' is asserted separately below.
        var reserved = new HashSet<char>
        {
            '.', ',', '#', 'A', '~',
            '+', '<', '>',
            ' ', 'S', 'F', 'U', 'E'
        };

        foreach (var key in MapFragment.FeatureGlyphs.Keys)
        {
            Assert.DoesNotContain(key, reserved);
        }
        Assert.DoesNotContain(MapFragment.CreatureWildcardGlyph, reserved);
    }

    [Fact]
    public void Glyph_table_covers_every_proposed_feature()
    {
        var expected = new[]
        {
            RoomFeatures.DeityAltar, RoomFeatures.SkillPedestal, RoomFeatures.TraitPedestal,
            RoomFeatures.FeatPedestal, RoomFeatures.RacePedestal, RoomFeatures.WeaponStand,
            RoomFeatures.ArmorStand, RoomFeatures.ItemCase, RoomFeatures.SmallFountain,
            RoomFeatures.BigFountain, RoomFeatures.MinorShrine, RoomFeatures.MajorShrine,
            RoomFeatures.RitualShrine
        };
        var present = MapFragment.FeatureGlyphs.Values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToHashSet();
        foreach (var f in expected)
        {
            Assert.Contains(f, present);
        }
    }

    [Fact]
    public void BindWildcardFeatures_assigns_one_outstanding_bit_per_wildcard()
    {
        var fragment = NewFeatureFragment("""
            #####
            #$.$#
            #...#
            #####
            """);
        var (room, lot, rp) = BuildFloorRoom(7);
        room.Features = RoomFeatures.DeityAltar | RoomFeatures.SmallFountain;
        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));
        Assert.NotNull(chunk);

        var bound = chunk!.FeaturePoints
            .Select(p => p.Feature)
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .ToHashSet();
        Assert.Equal(2, bound.Count);
        Assert.Contains(RoomFeatures.DeityAltar, bound);
        Assert.Contains(RoomFeatures.SmallFountain, bound);
    }

    [Fact]
    public void BindWildcardFeatures_skips_features_already_placed_concretely()
    {
        // The fragment authored '_' (DeityAltar) and a '$' wildcard. The room demands both
        // DeityAltar and Fountain. The wildcard must bind to Fountain — DeityAltar is
        // already satisfied by the concrete '_' cell.
        var fragment = NewFeatureFragment("""
            #####
            #_.$#
            #####
            """);
        var (room, lot, rp) = BuildFloorRoom(7);
        room.Features = RoomFeatures.DeityAltar | RoomFeatures.SmallFountain;
        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        var wildcard = chunk!.FeaturePoints.Single(p => p.Point.X == chunk.BoundingRectangle.TopLeft.X + 3);
        Assert.Equal(RoomFeatures.SmallFountain, wildcard.Feature);
    }

    [Fact]
    public void BindWildcardFeatures_leaves_extra_wildcards_unbound_when_no_outstanding_features()
    {
        var fragment = NewFeatureFragment("""
            #####
            #$$$#
            #####
            """);
        var (room, lot, rp) = BuildFloorRoom(7);
        room.Features = RoomFeatures.SmallFountain;
        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        var boundCount = chunk!.FeaturePoints.Count(p => p.Feature.HasValue);
        Assert.Equal(1, boundCount);
        Assert.Equal(RoomFeatures.SmallFountain, chunk.FeaturePoints.Single(p => p.Feature.HasValue).Feature);
    }

    [Fact]
    public void BindWildcardFeatures_no_op_when_room_has_no_placeable_features()
    {
        var fragment = NewFeatureFragment("""
            ###
            #$#
            ###
            """);
        var (room, lot, rp) = BuildFloorRoom(7);
        room.Features = RoomFeatures.Normal;
        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        Assert.All(chunk!.FeaturePoints, p => Assert.Null(p.Feature));
    }

    [Fact]
    public void Transforms_rotate_feature_and_creature_points()
    {
        // The source places '_' at (1,1) of a 3x4 payload and '%' at (1,2). After a 90° CW
        // rotation the payload is 4x3 and the positions should rotate accordingly.
        var source = new NormalMapFragment
        {
            Name = "transform-feature-source",
            Map = """
                ###
                #_#
                #%#
                ###
                """,
            NoRandomDoorways = true,
            NoMirror = true,
            NoTurnOver = true
        };
        source.EnsureInitialized();
        var variant = (NormalMapFragment)source.CreateVariant(MapFragmentTransform.Rotate90);

        var (room, lot, rp) = BuildFloorRoom(7);
        var chunk = variant.TryPlace(room, new LotPlacementContext(lot, rp));
        Assert.NotNull(chunk);

        var origin = chunk!.BoundingRectangle.TopLeft;
        // Source '_' at (1,1) in a 3-wide × 4-tall payload → rotated 90° CW lands at
        // (height-1-y, x) = (4-1-1, 1) = (2, 1) in the new 4-wide × 3-tall payload.
        var featurePoint = Assert.Single(chunk.FeaturePoints);
        Assert.Equal(RoomFeatures.DeityAltar, featurePoint.Feature);
        Assert.Equal(new Point((byte)(origin.X + 2), (byte)(origin.Y + 1)), featurePoint.Point);

        // Source '%' at (1,2) → rotated CW lands at (4-1-2, 1) = (1, 1).
        var creaturePoint = Assert.Single(chunk.CreaturePoints);
        Assert.Equal(new Point((byte)(origin.X + 1), (byte)(origin.Y + 1)), creaturePoint);
    }

    /// <summary>
    ///     A 3x3 walled box with a floor centre, placed inside a stone-floor room with
    ///     space on every side, should yield doorway candidates at the four wall midpoints
    ///     (corners are excluded).
    /// </summary>
    [Fact]
    public void Derives_doorways_at_four_midpoints_when_surrounded_by_floor()
    {
        var (room, lot, rp) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

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
        var (room, lot, rp) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

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
        var (room, lot, rp) = BuildSurroundedRoom(roomSize: 4, lotTopLeft: new Point(0, 0), lotSize: 3);
        var fragment = BuildBoxFragment(room.Game);

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

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
    ///     <see cref="MapFragment.NoRandomDoorways" /> opts the fragment out entirely —
    ///     no derivation, no throw, just an empty doorway list.
    /// </summary>
    [Fact]
    public void NoRandomDoorways_skips_derivation()
    {
        var (room, lot, rp) = BuildSurroundedRoom(roomSize: 5, lotTopLeft: new Point(1, 1), lotSize: 3);
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
        fragment.EnsureInitialized();

        var chunk = fragment.TryPlace(room, new LotPlacementContext(lot, rp));

        Assert.NotNull(chunk);
        Assert.Empty(chunk!.DoorwayPoints);
    }

    /// <summary>
    ///     Builds a square room of <paramref name="roomSize" /> filled with stone floor
    ///     and returns the room together with a bounding rectangle the caller can pass to
    ///     <see cref="MapFragment.TryPlace" /> to deterministically place a fragment of
    ///     the requested size at the requested top-left. The surrounding floor stays
    ///     in place so doorway derivation sees floor neighbours on every side; the
    ///     candidate fragment's wall writes inside the lot go through because the
    ///     bare placement context's <see cref="LotPlacementContext.ChunkCells" /> is
    ///     the empty shared sentinel (no cell looks owned).
    /// </summary>
    private static (RoomComponent Room, Rectangle Lot, RoomParameters Parameters) BuildSurroundedRoom(
        byte roomSize, Point lotTopLeft, byte lotSize)
    {
        var rowLine = new string('.', roomSize);
        var rows = new string[roomSize];
        for (var i = 0; i < roomSize; i++)
        {
            rows[i] = rowLine;
        }

        var room = TestHelper.BuildRoom(string.Join('\n', rows));
        return (room, new Rectangle(lotTopLeft, lotSize, lotSize), TestRoomParameters(room));
    }

    /// <summary>A 3x3 fragment: walls on the perimeter, single floor tile in the middle.</summary>
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
        fragment.EnsureInitialized();
        return fragment;
    }

    [Fact]
    public void Substitutes_annotated_glyph_with_one_of_the_listed_glyphs()
    {
        // Annotation '1' → "#." means each '1' cell is realized as either a stone wall or
        // floor at placement time. Across enough cells we'll see at least one of each.
        var fragment = new NormalMapFragment
        {
            Name = "annotation-substitute",
            Map = """
                #########
                #1111111#
                #1111111#
                #########
                """,
            Annotations = new Dictionary<char, string> { ['1'] = "#." },
            NoRandomDoorways = true,
            NoTransform = true
        };
        fragment.EnsureInitialized();
        var room = TestHelper.BuildRoom(new string('.', 11) + Environment.NewLine
            + string.Join(Environment.NewLine, Enumerable.Repeat(new string('.', 11), 10)));

        var chunk = fragment.TryPlace(room, new LotPlacementContext(new Rectangle(new Point(0, 0), 11, 11), TestRoomParameters(room)));
        Assert.NotNull(chunk);

        var origin = chunk!.BoundingRectangle.TopLeft;
        var floorCount = 0;
        var wallCount = 0;
        for (byte dy = 1; dy <= 2; dy++)
        {
            for (byte dx = 1; dx <= 7; dx++)
            {
                var feature = room.Terrain[room.PointToIndex![(byte)(origin.X + dx), (byte)(origin.Y + dy)]];
                if (feature == MapFeature.StoneWall)
                {
                    wallCount++;
                }
                else if (feature == MapFeature.StoneFloor)
                {
                    floorCount++;
                }
                else
                {
                    Assert.Fail($"unexpected terrain at ({origin.X + dx},{origin.Y + dy}): {feature}");
                }
            }
        }

        Assert.Equal(14, floorCount + wallCount);
        Assert.True(floorCount > 0, "expected at least one '.' substitution across 14 cells");
        Assert.True(wallCount > 0, "expected at least one '#' substitution across 14 cells");
    }

    [Fact]
    public void Single_substitute_is_deterministic()
    {
        var fragment = new NormalMapFragment
        {
            Name = "annotation-single",
            Map = """
                ###
                #1#
                ###
                """,
            Annotations = new Dictionary<char, string> { ['1'] = "." },
            NoRandomDoorways = true,
            NoTransform = true
        };
        fragment.EnsureInitialized();
        var room = TestHelper.BuildRoom(string.Join(Environment.NewLine, Enumerable.Repeat(new string('.', 7), 7)));

        var chunk = fragment.TryPlace(room, new LotPlacementContext(new Rectangle(new Point(0, 0), 7, 7), TestRoomParameters(room)));
        Assert.NotNull(chunk);

        var origin = chunk!.BoundingRectangle.TopLeft;
        var feature = room.Terrain[room.PointToIndex![(byte)(origin.X + 1), (byte)(origin.Y + 1)]];
        Assert.Equal(MapFeature.StoneFloor, feature);
    }
}
