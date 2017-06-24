using System;
using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class AddAbility : Effect
    {
        public AddAbility()
        {
        }

        public AddAbility(Game game)
            : base(game)
        {
        }

        public Ability Ability { get; set; }
        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new AddAbility(game) {Ability = Ability, Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}