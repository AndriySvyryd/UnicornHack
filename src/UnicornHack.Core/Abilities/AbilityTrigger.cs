namespace UnicornHack.Abilities
{
    public class AbilityTrigger : Trigger
    {
        public AbilityTrigger()
        {
        }

        public AbilityTrigger(Game game) : base(game)
        {
        }

        public int? TriggeredAbilityId { get; set; }
        public Ability Ability { get; set; }

        public override Trigger Copy(Game game)
            => new AbilityTrigger(game) { Ability = Ability.AddReference().Referenced };

        public override void Fire(AbilityActivationContext abilityContext)
        {
            using (var context = new AbilityActivationContext
            {
                Activator = abilityContext.Activator,
                Target = abilityContext.Target,
                EffectsToApply = abilityContext.EffectsToApply
            })
            {
                Ability.Activate(context);
            }
        }

        public override void Delete()
        {
            base.Delete();

            Ability.RemoveReference();
            Ability = null;
        }
    }
}