namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription DrainResistance = new PropertyDescription
        {
            Name = "drain resistance",
            PropertyType = typeof(int),
            MinValue = 0,
            MaxValue = 3
        };
    }
}