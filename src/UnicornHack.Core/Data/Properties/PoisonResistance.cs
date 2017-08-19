namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription PoisonResistance = new PropertyDescription
        {
            Name = "poison resistance",
            PropertyType = typeof(int),
            MinValue = 0,
            MaxValue = 3
        };
    }
}