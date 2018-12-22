using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Effects
{
    [Component(Id = (int)EntityComponent.Effect)]
    public class EffectComponent : GameComponent
    {
        private int? _affectedEntityId;
        private int? _sourceAbilityId;
        private int? _containingAbilityId;
        private int _durationTicks;
        private int? _expirationTick;
        private bool _shouldTargetActivator;
        private int? _amount;
        private EffectType _effectType;
        private ValueCombinationFunction _function;
        private string _targetName;
        private int? _targetEntityId;

        public EffectComponent()
            => ComponentId = (int)EntityComponent.Effect;

        public int? AffectedEntityId
        {
            get => _affectedEntityId;
            set => SetWithNotify(value, ref _affectedEntityId);
        }

        public int? SourceAbilityId
        {
            get => _sourceAbilityId;
            set => SetWithNotify(value, ref _sourceAbilityId);
        }

        public int? ContainingAbilityId
        {
            get => _containingAbilityId;
            set => SetWithNotify(value, ref _containingAbilityId);
        }

        // See EffectDuration
        public int DurationTicks
        {
            get => _durationTicks;
            set => SetWithNotify(value, ref _durationTicks);
        }

        public int? ExpirationTick
        {
            get => _expirationTick;
            set => SetWithNotify(value, ref _expirationTick);
        }

        public bool ShouldTargetActivator
        {
            get => _shouldTargetActivator;
            set => SetWithNotify(value, ref _shouldTargetActivator);
        }

        public int? Amount
        {
            get => _amount;
            set => SetWithNotify(value, ref _amount);
        }

        public EffectType EffectType
        {
            get => _effectType;
            set => SetWithNotify(value, ref _effectType);
        }

        public ValueCombinationFunction Function
        {
            get => _function;
            set => SetWithNotify(value, ref _function);
        }

        public string TargetName
        {
            get => _targetName;
            set => SetWithNotify(value, ref _targetName);
        }

        public int? TargetEntityId
        {
            get => _targetEntityId;
            set => SetWithNotify(value, ref _targetEntityId);
        }

        // TODO: Add application condition

        public EffectComponent AddToAbility(GameEntity abilityEntity)
        {
            var manager = abilityEntity.Manager;
            using (var entityReference = manager.CreateEntity())
            {
                var clone = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                clone.ShouldTargetActivator = ShouldTargetActivator;
                clone.DurationTicks = DurationTicks;
                clone.Amount = Amount;
                clone.EffectType = EffectType;
                clone.Function = Function;
                clone.TargetName = TargetName;
                clone.TargetEntityId = TargetEntityId;

                clone.ContainingAbilityId = abilityEntity.Id;

                if (EffectType == EffectType.AddAbility)
                {
                    Entity.Ability.AddToEffect(entityReference.Referenced);
                }

                entityReference.Referenced.Effect = clone;
                return clone;
            }
        }

        protected override void Clean()
        {
            _affectedEntityId = default;
            _sourceAbilityId = default;
            _containingAbilityId = default;
            _durationTicks = default;
            _expirationTick = default;
            _shouldTargetActivator = default;
            _amount = default;
            _effectType = default;
            _function = default;
            _targetName = default;
            _targetEntityId = default;

            base.Clean();
        }
    }
}
