namespace UnicornHack.Utils
{
    public struct Dimensions
    {
        public Dimensions(byte width, byte height)
        {
            Width = width;
            Height = height;
        }

        public byte Width { get; set; }
        public byte Height { get; set; }

        public bool Contains(Dimensions d) => Width >= d.Width && Height >= d.Height;
    }
}