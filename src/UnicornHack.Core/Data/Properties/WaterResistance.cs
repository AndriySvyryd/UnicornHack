namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription<int> WaterResistance =
            new PropertyDescription<int> {Name = "water resistance", MinValue = 0, MaxValue = 200, DefaultValue = 100};
    }
}