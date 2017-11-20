namespace UnicornHack.Effects
{
    public class Infect : Effect
    {
        public Infect()
        {
        }

        public Infect(Game game) : base(game)
        {
        }

        public int Strength { get; set; }

        public override Effect Copy(Game game) => new Infect(game) {Strength = Strength};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Infected(abilityContext) {Strength = Strength});
        }
    }
}