using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils.DataStructures
{
    /// <summary>
    ///     2d range tree for rectangle vertices. Vertices can be shared as long as the rectangles are different.
    /// </summary>
    public class RectangleRangeTree
    {
        private readonly AATreeStrict<byte, AATreeLax<byte, (Point, RectangleTracker)>> _tree =
            new AATreeStrict<byte, AATreeLax<byte, (Point, RectangleTracker)>>();

        public void InsertRange(IEnumerable<Rectangle> rectangles)
        {
            // TODO: sort points by Y first to improve subtree creation time
            foreach (var rectangle in rectangles)
            {
                if (!Insert(rectangle))
                {
                    throw new InvalidOperationException($"Couldn't add {rectangle}");
                }
            }
        }

        public bool Insert(Rectangle rectangle)
        {
            var topLeft = rectangle.TopLeft;
            var bottomRight = rectangle.BottomRight;
            if (!_tree.TryGetValue(topLeft.X, out var subtree))
            {
                subtree = new AATreeLax<byte, (Point, RectangleTracker)>();
                _tree[topLeft.X] = subtree;
            }

            var tracker = new RectangleTracker(rectangle);
            if (!subtree.Insert(topLeft.Y, (topLeft, tracker)))
            {
                return false;
            }

            if (topLeft.Y != bottomRight.Y && !subtree.Insert(bottomRight.Y, (rectangle.BottomLeft, tracker)))
            {
                return false;
            }

            if (topLeft.X != bottomRight.X)
            {
                if (!_tree.TryGetValue(bottomRight.X, out subtree))
                {
                    subtree = new AATreeLax<byte, (Point, RectangleTracker)>();
                    _tree[bottomRight.X] = subtree;
                }

                if (!subtree.Insert(bottomRight.Y, (bottomRight, tracker)))
                {
                    return false;
                }

                if (topLeft.Y != bottomRight.Y && !subtree.Insert(topLeft.Y, (rectangle.TopRight, tracker)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Remove(Rectangle rectangle)
        {
            var topLeft = rectangle.TopLeft;
            var bottomRight = rectangle.BottomRight;
            if (!_tree.TryGetValue(topLeft.X, out var subtree))
            {
                return false;
            }

            var tracker = new RectangleTracker(rectangle);
            if (!subtree.Remove(topLeft.Y, (topLeft, tracker)))
            {
                return false;
            }

            if (topLeft.Y != bottomRight.Y
                && !subtree.Remove(bottomRight.Y, (new Point(topLeft.X, bottomRight.Y), tracker)))
            {
                return false;
            }

            if (topLeft.X != bottomRight.X)
            {
                if (!_tree.TryGetValue(bottomRight.X, out subtree))
                {
                    return false;
                }

                if (!subtree.Remove(topLeft.Y, (new Point(bottomRight.X, topLeft.Y), tracker)))
                {
                    return false;
                }

                if (topLeft.Y != bottomRight.Y && !subtree.Remove(bottomRight.Y, (bottomRight, tracker)))
                {
                    return false;
                }
            }

            return true;
        }

        private int _currentResultSet;

        public IEnumerable<Rectangle> GetOverlappingCorners(Rectangle rectangle)
        {
            if (_currentResultSet++ < 0)
            {
                // Reset the result sets if the number overflows
                _currentResultSet = 1;
                foreach (var _ in GetAll((_, t) => t.Item2.ResultSet = 1))
                {
                }
            }

            // TODO: Consider using dynamic fractional cascading for perf
            return _tree.GetRange(rectangle.TopLeft.X, rectangle.BottomRight.X, (x, t1) => t1).SelectMany(subtree =>
                subtree.GetRange(rectangle.TopLeft.Y, rectangle.BottomRight.Y, (y, t) =>
                {
                    // Ignore duplicates
                    if (t.Item2.ResultSet == _currentResultSet)
                    {
                        return null;
                    }

                    t.Item2.ResultSet = _currentResultSet;
                    return t.Item2.Rectangle;
                })).Where(r => r != null).Select(r => r.Value);
        }

        private IEnumerable<TResult> GetAll<TResult>(Func<byte, (Point, RectangleTracker), TResult> selector) =>
            _tree.GetAll((x, t) => t).SelectMany(subtree => subtree.GetAll(selector));


        private class RectangleTracker
        {
            public RectangleTracker(Rectangle rectangle) => Rectangle = rectangle;

            public int ResultSet { get; set; }
            public Rectangle? Rectangle { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var tracker = obj as RectangleTracker;
                if (ReferenceEquals(tracker, null))
                {
                    return false;
                }

                return Rectangle.Equals(tracker.Rectangle);
            }

            public override int GetHashCode() => Rectangle.GetHashCode();
        }
    }
}
