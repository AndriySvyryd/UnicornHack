using UnicornHack.Abilities;
using UnicornHack.Generation;

namespace UnicornHack.Effects
{
    public class AddAbility : Effect
    {
        public AddAbility()
        {
        }

        public AddAbility(Game game) : base(game)
        {
        }

        public AbilityDefinition Ability { get; set; }

        public override Effect Copy(Game game)
            => new AddAbility(game) {Ability = Ability.Copy(game), Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            var newEffect = new AddedAbility(abilityContext)
            {
                Duration = Duration,
                Ability = Ability.Instantiate(Game).AddReference().Referenced
            };
            newEffect.Add();
            abilityContext.TargetEntity.Add(newEffect.Ability);
        }

        public override void Delete()
        {
            base.Delete();

            Ability.Delete();
            Ability = null;
        }
    }
}