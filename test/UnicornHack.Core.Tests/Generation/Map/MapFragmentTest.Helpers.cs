using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Shared helpers used by every <see cref="MapFragmentTest" /> partial: fragment
///     factories, lot context builders, and the boilerplate that spins up a single-room
///     <see cref="EmptyLevelLayout" /> game for placement assertions.
/// </summary>
public partial class MapFragmentTest
{
    private static TestNormalMapFragment InitializeFragment(string map)
    {
        var game = TestHelper.CreateGame(1u);
        var fragment = new TestNormalMapFragment { Map = map, NoRandomDoorways = true };
        fragment.EnsureInitialized();
        return fragment;
    }

    private static NormalMapFragment NewFragment(string map, IEnumerable<string>? authoredTags = null)
    {
        var f = new NormalMapFragment
        {
            Name = "test-fragment",
            Map = map
        };
        if (authoredTags is not null)
        {
            f.Tags = [..authoredTags];
        }

        f.EnsureInitialized();
        return f;
    }

    private static NormalMapFragment NewFragment(IEnumerable<string>? tags = null, IEnumerable<string>? antiTags = null)
    {
        var f = new NormalMapFragment
        {
            Name = "test-fragment",
            Map = """
                ###
                #.#
                ###
                """
        };
        var seed = new List<string>();
        if (tags is not null)
        {
            seed.AddRange(tags);
        }
        if (antiTags is not null)
        {
            // Negative tags are moved to AntiTags by EnsureInitialized via the `!`-prefix.
            seed.AddRange(antiTags.Select(t => "!" + t));
        }
        if (seed.Count > 0)
        {
            f.Tags = [..seed];
        }
        f.EnsureInitialized();
        return f;
    }

    private static LotPlacementContext OpenCtx(RoomComponent room, Rectangle lot,
        Direction? target = null, Dimensions? minLotSize = null)
    {
        var emptySpace = new RectanglePartition(room.BoundingRectangle);
        emptySpace.Insert(lot);
        return new LotPlacementContext(
            lot,
            TestHelper.CreateRoomParameters(room),
            TargetEdge: target,
            EmptySpace: emptySpace,
            MinLotSize: minLotSize ?? new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: null);
    }

    private static RectanglePartition OpenPartition(RoomComponent room, Rectangle lot)
    {
        var p = new RectanglePartition(room.BoundingRectangle);
        p.Insert(lot);
        return p;
    }

    private static (RoomComponent Room, Game Game) BuildRoom(
        byte roomSize = 7, MapFeature fill = MapFeature.Default)
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var branch = new Branch
        {
            Name = "map-fragment-test",
            Length = 1,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        var branchEntity = branch.Instantiate(game);
        game.Branches.Add(branchEntity);

        var gameLevel = LevelGenerator.CreateEmpty(branchEntity, depth: 1, seed: 1u, game.Manager);
        var room = LevelLayout.GetEmptyEntranceRoom(gameLevel, game.Manager);
        LevelLayout.AllocateTerrain(room, roomSize, roomSize, fill);

        return (room, game);
    }

    /// <summary>
    ///     Public passthrough for other test classes that need the same
    ///     single-room <see cref="EmptyLevelLayout" /> bootstrap.
    /// </summary>
    public static (RoomComponent Room, Game Game) BuildRoomForBlobTest(byte roomSize)
        => BuildRoom(roomSize, MapFeature.Default);
}
