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

        public int? AbilityToTriggerId { get; set; }
        public Ability AbilityToTrigger { get; set; }

        public override Trigger Copy(Game game)
            => new AbilityTrigger(game) { AbilityToTrigger = AbilityToTrigger.AddReference().Referenced };

        public override void Fire(AbilityActivationContext abilityContext)
        {
            using (var context = new AbilityActivationContext
            {
                Activator = abilityContext.Activator,
                TargetEntity = abilityContext.TargetEntity,
                TargetCell = abilityContext.TargetCell,
                EffectsToApply = abilityContext.EffectsToApply
            })
            {
                AbilityToTrigger.Activate(context);
            }
        }

        public override void Delete()
        {
            base.Delete();

            AbilityToTrigger.RemoveReference();
            AbilityToTrigger = null;
        }
    }
}