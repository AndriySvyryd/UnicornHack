using UnicornHack.Utils;

namespace UnicornHack.Effects
{
    /// Represents the result from an effect application
    public abstract class AppliedEffect : IReferenceable
    {
        protected AppliedEffect()
        {
        }

        protected AppliedEffect(AbilityActivationContext abilityContext)
        {
            SourceAbility = abilityContext.Ability.AddReference().Referenced;
            SourceAbility.ActiveEffects.Add(this);
            Game = abilityContext.Target.Game;
            Entity = abilityContext.Target;
            Id = abilityContext.Target.Game.NextAppliedEffectId++;
        }

        public int? Duration { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public int? EntityId { get; private set; }

        public Game Game { get; set; }
        public Entity Entity { get; set; }

        public Ability SourceAbility { get; set; }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<AppliedEffect> AddReference() => new TransientReference<AppliedEffect>(this);

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                Game.Repository.Delete(this);
            }
        }

        public virtual void Remove()
        {
            Entity.ActiveEffects.Remove(this);
            SourceAbility.ActiveEffects.Remove(this);
            SourceAbility?.RemoveReference();
            SourceAbility = null;
            RemoveReference();
        }
    }
}