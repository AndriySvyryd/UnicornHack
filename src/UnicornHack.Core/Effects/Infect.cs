using UnicornHack.Abilities;

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

        public Infect(Infect effect, Game game)
            : base(effect, game)
            => Strength = Strength;

        public int Strength { get; set; }

        public override Effect Copy(Game game) => new Infect(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Infected(abilityContext, TargetActivator) {Strength = Strength});
        }
    }
}