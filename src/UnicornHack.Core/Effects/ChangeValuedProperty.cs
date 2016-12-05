namespace UnicornHack.Effects
{
    public class ChangeValuedProperty : Effect
    {
        public ChangeValuedProperty()
        {
        }

        public ChangeValuedProperty(Game game)
            : base(game)
        {
        }

        public string PropertyName { get; set; }
        public string Value { get; set; }
        public bool IsAbsolute { get; set; }

        public override Effect Instantiate(Game game)
            => new ChangeValuedProperty(game)
            {
                PropertyName = PropertyName,
                Value = Value,
                IsAbsolute = IsAbsolute
            };
    }
}