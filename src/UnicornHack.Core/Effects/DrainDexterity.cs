using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class DrainDexterity : Effect
    {
        public DrainDexterity()
        {
        }

        public DrainDexterity(Game game)
            : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Instantiate(Game game)
            => new DrainDexterity(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new System.NotImplementedException();
        }
    }
}