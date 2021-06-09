using System;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Effects
{
    [Component(Id = (int)EntityComponent.Effect)]
    public class EffectComponent : GameComponent
    {
        private int? _affectedEntityId;
        private int? _sourceEffectId;
        private int? _sourceAbilityId;
        private int? _containingAbilityId;
        private string _durationAmount;
        private int? _expirationTick;
        private int? _expirationXp;
        private bool _shouldTargetActivator;
        private int? _appliedAmount;
        private string _amount;
        private string _secondaryAmount;
        private EffectType _effectType;
        private ValueCombinationFunction _function;
        private string _targetName;
        private int? _targetEntityId;
        private EffectDuration _duration;

        public EffectComponent()
            => ComponentId = (int)EntityComponent.Effect;

        public int? AffectedEntityId
        {
            get => _affectedEntityId;
            set => SetWithNotify(value, ref _affectedEntityId);
        }

        public int? SourceEffectId
        {
            get => _sourceEffectId;
            set => SetWithNotify(value, ref _sourceEffectId);
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

        public EffectDuration Duration
        {
            get => _duration;
            set => SetWithNotify(value, ref _duration);
        }

        public Func<GameEntity, GameEntity, float> DurationAmountFunction { get; set; }

        public string DurationAmount
        {
            get => _durationAmount;
            set
            {
                SetWithNotify(value, ref _durationAmount);
                DurationAmountFunction = null;
            }
        }

        public int? ExpirationTick
        {
            get => _expirationTick;
            set => SetWithNotify(value, ref _expirationTick);
        }

        public int? ExpirationXp
        {
            get => _expirationXp;
            set => SetWithNotify(value, ref _expirationXp);
        }

        public bool ShouldTargetActivator
        {
            get => _shouldTargetActivator;
            set => SetWithNotify(value, ref _shouldTargetActivator);
        }

        public int? AppliedAmount
        {
            get => _appliedAmount;
            set => SetWithNotify(value, ref _appliedAmount);
        }

        public Func<GameEntity, GameEntity, float> AmountFunction { get; set; }
        public Func<GameEntity, GameEntity, float> SecondaryAmountFunction { get; set; }

        public string Amount
        {
            get => _amount;
            set
            {
                SetWithNotify(value, ref _amount);
                AmountFunction = null;
            }
        }

        public string SecondaryAmount
        {
            get => _secondaryAmount;
            set
            {
                SetWithNotify(value, ref _secondaryAmount);
                SecondaryAmountFunction = null;
            }
        }

        public EffectType EffectType
        {
            get => _effectType;
            set => SetWithNotify(value, ref _effectType);
        }

        public ValueCombinationFunction CombinationFunction
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

        public EffectComponent AddToAbility(GameEntity abilityEntity)
        {
            var manager = abilityEntity.Manager;
            using (var entityReference = manager.CreateEntity())
            {
                var clone = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                clone.ShouldTargetActivator = ShouldTargetActivator;
                clone.Duration = Duration;
                clone.DurationAmount = DurationAmount;
                clone.AppliedAmount = AppliedAmount;
                clone.Amount = Amount;
                clone.EffectType = EffectType;
                clone.CombinationFunction = CombinationFunction;
                clone.TargetName = TargetName;
                clone.TargetEntityId = TargetEntityId;

                clone.ContainingAbilityId = abilityEntity.Id;

                if (EffectType == EffectType.AddAbility)
                {
                    Entity.Ability?.AddToEffect(entityReference.Referenced);
                }

                entityReference.Referenced.Effect = clone;
                return clone;
            }
        }

        protected override void Clean()
        {
            _affectedEntityId = default;
            _sourceEffectId = default;
            _sourceAbilityId = default;
            _containingAbilityId = default;
            _duration = default;
            DurationAmountFunction = default;
            _durationAmount = default;
            _expirationTick = default;
            _expirationXp = default;
            _shouldTargetActivator = default;
            _appliedAmount = default;
            AmountFunction = default;
            _amount = default;
            SecondaryAmountFunction = default;
            _secondaryAmount = default;
            _effectType = default;
            _function = default;
            _targetName = default;
            _targetEntityId = default;

            base.Clean();
        }
    }
}
