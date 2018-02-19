namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription<int> Weight =
            new PropertyDescription<int>
            {
                Name = "weight",
                Desciption = "Weight in units of 100 grams",
                MinValue = 0,
                DefaultValue = 1
            };
    }
}