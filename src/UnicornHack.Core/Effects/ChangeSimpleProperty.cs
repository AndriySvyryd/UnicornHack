using System;
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
        public bool IsAdded { get; set; }

        public override Effect Instantiate(Game game)
            => new ChangeSimpleProperty(game)
            {
                PropertyName = PropertyName,
                IsAdded = IsAdded
            };

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}