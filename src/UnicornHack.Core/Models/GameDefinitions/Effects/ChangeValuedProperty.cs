namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class ChangeValuedProperty : AbilityEffect
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public bool IsAbsolute { get; set; }
    }
}
