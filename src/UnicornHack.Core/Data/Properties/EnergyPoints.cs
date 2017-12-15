namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription<int> EnergyPoints =
            new PropertyDescription<int>
            {
                Name = "energy points",
                IsCalculated = false,
                MinValue = 0,
                DefaultValue = 100
            };
    }
}