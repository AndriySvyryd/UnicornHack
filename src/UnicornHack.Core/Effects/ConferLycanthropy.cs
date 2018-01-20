using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class ConferLycanthropy : Effect
    {
        public ConferLycanthropy()
        {
        }

        public ConferLycanthropy(Game game) : base(game)
        {
        }

        public ConferLycanthropy(ConferLycanthropy effect, Game game)
            : base(effect, game)
            => VariantName = effect.VariantName;

        public string VariantName { get; set; }

        public override Effect Copy(Game game) => new ConferLycanthropy(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new LycanthropyConfered(abilityContext, TargetActivator) {VariantName = VariantName});
        }
    }
}