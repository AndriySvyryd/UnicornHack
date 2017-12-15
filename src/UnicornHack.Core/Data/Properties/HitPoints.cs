namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription<int> HitPoints =
            new PropertyDescription<int> {Name = "hit points", IsCalculated = false, MinValue = 0, DefaultValue = 100};
    }
}