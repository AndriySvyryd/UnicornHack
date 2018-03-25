using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.DataStructures
{
    public class RectangleIntervalTree
    {
        private readonly RectangleRangeTree _rectangleRangeTree;
        private readonly IntervalTree _yIntervalTree;
        private readonly YBoundedIntervalTree _xIntervalTree;

        public RectangleIntervalTree(Rectangle boundingRectangle)
        {
            _rectangleRangeTree = new RectangleRangeTree();
            _xIntervalTree = new YBoundedIntervalTree(boundingRectangle.XProjection);
            _yIntervalTree = new IntervalTree(boundingRectangle.YProjection);
        }

        public Rectangle BoundingRectangle =>
            new Rectangle(_xIntervalTree.BoundingSegment, _yIntervalTree.BoundingSegment);

        public void InsertRange(IReadOnlyList<Rectangle> rectangles)
        {
            _rectangleRangeTree.InsertRange(rectangles);
            _xIntervalTree.InsertRange(rectangles);
            _yIntervalTree.InsertRange(rectangles);
        }

        public bool Insert(Rectangle rectangle) => _rectangleRangeTree.Insert(rectangle) &&
                                                   _xIntervalTree.Insert(rectangle) && _yIntervalTree.Insert(rectangle);

        public bool Remove(Rectangle rectangle) => _rectangleRangeTree.Remove(rectangle) &&
                                                   _xIntervalTree.Remove(rectangle) && _yIntervalTree.Remove(rectangle);

        public IEnumerable<Rectangle> GetOverlapping(Rectangle rectangle) => _rectangleRangeTree
            .GetOverlappingCorners(rectangle).Concat(_xIntervalTree.GetEnclosing(rectangle))
            .Concat(_yIntervalTree.GetEnclosing(rectangle));
    }
}
