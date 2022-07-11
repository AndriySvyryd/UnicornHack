namespace UnicornHack.Utils.DataStructures;

/// <summary>
///     X-axis interval tree that also filters on Y coordinates
/// </summary>
public class YBoundedIntervalTree : IntervalTreeBase
{
    public YBoundedIntervalTree(Segment boundingSegment) : base(boundingSegment)
    {
    }

    public override bool Insert(Rectangle rectangle) => Insert(rectangle.XProjection, rectangle);
    public override bool Remove(Rectangle rectangle) => Remove(rectangle.XProjection, rectangle);

    protected override NodeBase CreateNode(byte key) => new Node(key);

    protected override bool SubtreeInsert(Rectangle rectangle, NodeBase node) =>
        ((Node)node).IntersectingRectangles.Insert(rectangle);

    protected override bool SubtreeRemove(Rectangle rectangle, NodeBase node) =>
        ((Node)node).IntersectingRectangles.Remove(rectangle);

    public IEnumerable<Rectangle> GetEnclosing(Rectangle rectangle)
    {
        var projection = rectangle.XProjection;
        var otherProjection = rectangle.YProjection;
        if (!BoundingSegment.Contains(projection))
        {
            throw new ArgumentOutOfRangeException($"Rectangle {rectangle} outside of the X bounding segment {BoundingSegment}");
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
                results.AddRange(node.IntersectingRectangles.GetOverlappingCorners(
                    new Rectangle(new Segment(BoundingSegment.Beginning, (byte)(projection.Beginning - 1)),
                        otherProjection)));
                node = (Node?)node.Left;
            }
            else if (projection.Beginning > node.Key)
            {
                results.AddRange(node.IntersectingRectangles.GetOverlappingCorners(
                    new Rectangle(new Segment((byte)(projection.End + 1), BoundingSegment.End), otherProjection)));
                node = (Node?)node.Right;
            }
            else
            {
                results.AddRange(node.IntersectingRectangles
                    .GetOverlappingCorners(new Rectangle(
                        new Segment(BoundingSegment.Beginning, (byte)(projection.Beginning - 1)), otherProjection))
                    .Where(r => r.BottomRight.X > projection.End));
                break;
            }
        }

        return results;
    }

    protected class Node : NodeBase
    {
        public RectangleRangeTree IntersectingRectangles;

        public Node(byte key) : base(key)
        {
            IntersectingRectangles = new RectangleRangeTree();
        }
    }
}
