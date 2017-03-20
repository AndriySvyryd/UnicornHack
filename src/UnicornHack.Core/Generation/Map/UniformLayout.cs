using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class UniformLayout : Layout
    {
        private Dimensions _lotSize;

        public virtual Dimensions MaxLotSize { get; set; } = new Dimensions(20, 20);
        public virtual Dimensions MinLotSize { get; set; } = new Dimensions(6, 6);
        protected virtual int LotPlacementAttempts { get; set; } = 10;

        public override void Fill(Level level, EncompassingMapFragment fragment)
        {
            _lotSize = MaxLotSize;
            base.Fill(level, fragment);
        }

        protected override Rectangle? SelectNextLot(RectangleIntervalTree placedFragments, SimpleRandom random)
        {
            var boundingRectangle = placedFragments.BoundingRectangle;
            while (_lotSize.Width > boundingRectangle.Width
                   || _lotSize.Height > boundingRectangle.Height)
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
    }
}