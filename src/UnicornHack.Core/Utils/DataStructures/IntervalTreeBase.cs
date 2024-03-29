namespace UnicornHack.Utils.DataStructures;

public abstract class IntervalTreeBase
{
    protected IntervalTreeBase(Segment boundingSegment)
    {
        BoundingSegment = boundingSegment;
        Root = CreateNode(boundingSegment.MidPoint);
    }

    public Segment BoundingSegment
    {
        get;
    }

    protected NodeBase Root
    {
        get;
    }

    public void InsertRange(IEnumerable<Rectangle> rectangles)
    {
        foreach (var rectangle in rectangles)
        {
            if (!Insert(rectangle))
            {
                throw new InvalidOperationException($"Couldn't add {rectangle}");
            }
        }
    }

    public abstract bool Insert(Rectangle rectangle);

    protected bool Insert(Segment projection, Rectangle rectangle)
    {
        var node = Root;
        var boundingSegment = BoundingSegment;
        if (!boundingSegment.Contains(projection))
        {
            throw new ArgumentOutOfRangeException($"Rectangle {rectangle} outside of the bounding segment {BoundingSegment}");
        }

        if (projection.Length < 3)
        {
            // Ignore short segments as they can't enclose anything
            return true;
        }

        while (true)
        {
            if (projection.End < node.Key)
            {
                boundingSegment = new Segment(boundingSegment.Beginning, (byte)(node.Key - 1));
                node.Left ??= CreateNode(boundingSegment.MidPoint);

                node = node.Left;
                continue;
            }

            if (projection.Beginning > node.Key)
            {
                boundingSegment = new Segment((byte)(node.Key + 1), boundingSegment.End);
                node.Right ??= CreateNode(boundingSegment.MidPoint);

                node = node.Right;
                continue;
            }

            return SubtreeInsert(rectangle, node);
        }
    }

    public abstract bool Remove(Rectangle rectangle);

    protected bool Remove(Segment projection, Rectangle rectangle)
    {
        if (projection.Length < 3)
        {
            // Ignore short segments as they can't enclose anything
            return true;
        }

        var node = Root;
        if (!BoundingSegment.Contains(projection))
        {
            throw new ArgumentOutOfRangeException($"Rectangle {rectangle} outside of the bounding segment {BoundingSegment}");
        }

        while (true)
        {
            if (node == null)
            {
                return false;
            }

            if (projection.End < node.Key)
            {
                node = node.Left;
                continue;
            }

            if (projection.Beginning > node.Key)
            {
                node = node.Right;
                continue;
            }

            return SubtreeRemove(rectangle, node);
        }
    }

    protected abstract NodeBase CreateNode(byte key);
    protected abstract bool SubtreeInsert(Rectangle rectangle, NodeBase node);
    protected abstract bool SubtreeRemove(Rectangle rectangle, NodeBase node);

    protected abstract class NodeBase
    {
        public byte Key;
        public NodeBase? Left;
        public NodeBase? Right;

        protected NodeBase(byte key)
        {
            Key = key;
        }
    }
}
