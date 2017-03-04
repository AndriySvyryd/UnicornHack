namespace UnicornHack.Utils
{
    /// <summary>
    ///     A rectangle aligned with the axis
    /// </summary>
    public struct Rectangle
    {
        public Rectangle(Point topLeft, Point bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public Rectangle(Point topLeft, byte width, byte height)
        {
            TopLeft = topLeft;
            BottomRight = new Point((byte)(topLeft.X + width), (byte)(topLeft.Y + height));
        }

        public Point TopLeft;
        public Point BottomRight;
        public byte Width => (byte)(BottomRight.X - TopLeft.X);
        public byte Height => (byte)(BottomRight.Y - TopLeft.Y);
    }
}