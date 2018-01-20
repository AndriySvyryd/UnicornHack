using UnicornHack.Abilities;
using UnicornHack.Generation;

namespace UnicornHack.Effects
{
    public class AddAbility : DurationEffect
    {
        public AddAbility()
        {
        }

        public AddAbility(Game game) : base(game)
        {
        }

        public AddAbility(AddAbility effect, Game game)
            : base(effect, game)
            => Ability = effect.Ability.Copy(game);

        public AbilityDefinition Ability { get; set; }

        public override Effect Copy(Game game) => new AddAbility(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            var newEffect = new AddedAbility(abilityContext, TargetActivator)
            {
                Duration = Duration,
                Ability = Ability.Instantiate(Game).AddReference().Referenced
            };
            newEffect.Add();
            (TargetActivator ? abilityContext.Activator : abilityContext.TargetEntity).Add(newEffect.Ability);
        }

        public override void Delete()
        {
            base.Delete();

            Ability.Delete();
            Ability = null;
        }
    }
}