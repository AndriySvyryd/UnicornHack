namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription Enchantment = new PropertyDescription
        {
            Name = "enchantment",
            PropertyType = typeof(int),
            MinValue = -5,
            MaxValue = 5
        };
    }
}