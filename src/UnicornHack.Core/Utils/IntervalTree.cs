using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils
{
    /// <summary>
    ///     Y-axis interval tree
    /// </summary>
    public class IntervalTree : IntervalTreeBase
    {
        public IntervalTree(Segment boundingSegment) : base(boundingSegment)
        {
        }

        public override bool Insert(Rectangle rectangle) => Insert(rectangle.YProjection, rectangle);
        public override bool Remove(Rectangle rectangle) => Remove(rectangle.YProjection, rectangle);

        protected override NodeBase NewNode(byte key) => new Node(key);

        protected override bool SubtreeInsert(Rectangle rectangle, NodeBase node) =>
            ((Node)node).Beginnings.Insert(rectangle.TopLeft.Y, rectangle) &&
            ((Node)node).Ends.Insert(rectangle.BottomRight.Y, rectangle);

        protected override bool SubtreeRemove(Rectangle rectangle, NodeBase node) =>
            ((Node)node).Beginnings.Remove(rectangle.TopLeft.Y, rectangle) &&
            ((Node)node).Ends.Remove(rectangle.BottomRight.Y, rectangle);

        public IEnumerable<Rectangle> GetEnclosing(Rectangle rectangle)
        {
            var projection = rectangle.YProjection;
            var otherProjection = rectangle.XProjection;
            if (!BoundingSegment.Contains(projection))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (projection.Beginning == BoundingSegment.Beginning || projection.End == BoundingSegment.End)
            {
                return Enumerable.Empty<Rectangle>();
            }

            var results = new List<Rectangle>();
            var node = (Node)Root;
            while (node != null)
            {
                if (projection.End < node.Key)
                {
                    results.AddRange(node.Beginnings
                        .GetRange(BoundingSegment.Beginning, (byte)(projection.Beginning - 1), (p, r) => r)
                        .Where(r => r.XProjection.Overlaps(otherProjection)));
                    node = (Node)node.Left;
                }
                else if (projection.Beginning > node.Key)
                {
                    results.AddRange(node.Ends.GetRange((byte)(projection.End + 1), BoundingSegment.End, (p, r) => r)
                        .Where(r => r.XProjection.Overlaps(otherProjection)));
                    node = (Node)node.Right;
                }
                else
                {
                    results.AddRange(node.Beginnings
                        .GetRange(BoundingSegment.Beginning, (byte)(projection.Beginning - 1), (p, r) => r).Where(r =>
                            r.BottomRight.Y > projection.End && r.XProjection.Overlaps(otherProjection)));
                    break;
                }
            }

            return results;
        }

        protected class Node : NodeBase
        {
            public AATreeLax<byte, Rectangle> Beginnings;
            public AATreeLax<byte, Rectangle> Ends;

            public Node(byte key) : base(key)
            {
                Beginnings = new AATreeLax<byte, Rectangle>();
                Ends = new AATreeLax<byte, Rectangle>();
            }
        }
    }
}