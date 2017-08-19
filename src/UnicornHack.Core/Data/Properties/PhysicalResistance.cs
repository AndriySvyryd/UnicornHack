namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription PhysicalResistance = new PropertyDescription
        {
            Name = "physical resistance",
            PropertyType = typeof(int),
            MinValue = 0,
            MaxValue = 200
        };
    }
}