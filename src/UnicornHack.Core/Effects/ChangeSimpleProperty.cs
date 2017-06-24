using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class ChangeSimpleProperty : Effect
    {
        public ChangeSimpleProperty()
        {
        }

        public ChangeSimpleProperty(Game game)
            : base(game)
        {
        }

        public string PropertyName { get; set; }
        public bool Remove { get; set; }
        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new ChangeSimpleProperty(game)
            {
                PropertyName = PropertyName,
                Remove = Remove,
                Duration = Duration
            };

        public override void Apply(AbilityActivationContext abilityContext)
        {
        }
    }
}