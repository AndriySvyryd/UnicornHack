namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription DangerAwareness = new PropertyDescription
        {
            Name = "danger awareness",
            PropertyType = typeof(int),
            MinValue = 0,
            MaxValue = 3
        };
    }
}