using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map;

public class UniformLayout : Layout, ICSScriptSerializable
{
    private Dimensions _lotSize;

    protected static Dimensions DefaultMaxLotSize = new(15, 15);

    public Dimensions MaxLotSize
    {
        get;
        set;
    } = DefaultMaxLotSize;

    protected static Dimensions DefaultMinLotSize = new(5, 5);

    public Dimensions MinLotSize
    {
        get;
        set;
    } = DefaultMinLotSize;

    protected int LotPlacementAttempts
    {
        get;
        set;
    } = 16;

    public override List<Room> Fill(LevelComponent level, DefiningMapFragment fragment)
    {
        _lotSize = MaxLotSize;
        return base.Fill(level, fragment);
    }

    protected override Rectangle? SelectNextLot(RectangleIntervalTree placedFragments, SimpleRandom random)
    {
        var boundingRectangle = placedFragments.BoundingRectangle;
        while (_lotSize.Width > boundingRectangle.Width || _lotSize.Height > boundingRectangle.Height)
        {
            _lotSize = new Dimensions((byte)(_lotSize.Width - 1), (byte)(_lotSize.Height - 1));
        }

        while (_lotSize.Contains(MinLotSize))
        {
            // TODO: Optimize using
            // "Polygon Decomposition". Handbook of Computational Geometry. p. 491 or
            // "Graph-Theoretic Solutions to Computational Geometry Problems"
            // https://stackoverflow.com/questions/5919298/algorithm-for-finding-the-fewest-rectangles-to-cover-a-set-of-rectangles-without

            for (var attempt = 0; attempt < LotPlacementAttempts; attempt++)
            {
                var x1 = (byte)random.Next(boundingRectangle.TopLeft.X,
                    boundingRectangle.BottomRight.X - _lotSize.Width + 1);
                var y1 = (byte)random.Next(boundingRectangle.TopLeft.Y,
                    boundingRectangle.BottomRight.Y - _lotSize.Height + 1);
                var potentialLot = new Rectangle(new Point(x1, y1), (byte)(_lotSize.Width - 1),
                    (byte)(_lotSize.Height - 1));
                if (!placedFragments.GetOverlapping(potentialLot).Any())
                {
                    return potentialLot;
                }
            }

            _lotSize = new Dimensions((byte)(_lotSize.Width - 1), (byte)(_lotSize.Height - 1));
        }

        return null;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<UniformLayout>(GetPropertyConditions<UniformLayout>());

    protected static Dictionary<string, Func<TUniformLayout, object?, bool>> GetPropertyConditions<TUniformLayout>()
        where TUniformLayout : UniformLayout => new()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        { nameof(Coverage), (_, v) => (float)v! != DefaultCoverage },
        { nameof(MaxRoomCount), (_, v) => (byte)v! != DefaultMaxRoomCount },
        { nameof(MaxLotSize), (_, v) => !DefaultMaxLotSize.Equals(v) },
        { nameof(MinLotSize), (_, v) => !DefaultMinLotSize.Equals(v) }
    };

    ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
}
