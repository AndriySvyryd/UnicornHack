using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainConstitution : Effect
    {
        public DrainConstitution()
        {
        }

        public DrainConstitution(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainConstitution(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}