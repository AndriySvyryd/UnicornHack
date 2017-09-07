namespace UnicornHack.Data.Properties
{
    public static partial class PropertyData
    {
        public static readonly PropertyDescription<int> HitPointMaximum =
            new PropertyDescription<int> {Name = "hit point maximum", MinValue = 0, DefaultValue = 100};
    }
}