namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription Largeness = new PropertyDescription
        {
            Name = "largeness",
            PropertyType = typeof(Size),
            DefaultValue = Size.Medium,
            MinValue = Size.Tiny,
            MaxValue = Size.Gigantic
        };
    }
}