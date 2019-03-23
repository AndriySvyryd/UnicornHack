using System;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;

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
        private int? _amount;
        private string _amountExpression;
        private int? _secondaryAmount;
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

        public string DurationAmount
        {
            get => _durationAmount;
            set => SetWithNotify(value, ref _durationAmount);
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

        public int? Amount
        {
            get => _amount;
            set => SetWithNotify(value, ref _amount);
        }

        public string AmountExpression
        {
            get => _amountExpression;
            set => SetWithNotify(value, ref _amountExpression);
        }

        public int? SecondaryAmount
        {
            get => _secondaryAmount;
            set => SetWithNotify(value, ref _secondaryAmount);
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

        public int GetActualDurationAmount()
        {
            if (DurationAmount == null)
            {
                return 0;
            }

            if (int.TryParse(DurationAmount, out var intDuration))
            {
                return intDuration;
            }

            throw new InvalidOperationException("Can't parse duration " + DurationAmount);
        }

        public int GetActualAmount(GameEntity activator)
        {
            if (AmountExpression == null)
            {
                return Amount.Value;
            }

            if (int.TryParse(AmountExpression, out var intAmount))
            {
                return intAmount;
            }

            var parts = AmountExpression.Split('*');
            if (parts.Length != 2)
            {
                throw new InvalidOperationException(AmountExpression + " unsupported operation");
            }

            if (!int.TryParse(parts[0], out var baseFactor))
            {
                throw new InvalidOperationException(AmountExpression + " unsupported factor");
            }

            if (parts[1] != AbilityComponent.WeaponScalingDelay)
            {
                throw new InvalidOperationException(AmountExpression + " unsupported scaling");
            }

            // TODO: Move scaling formula
            var item = Entity.Manager.FindEntity(ContainingAbilityId).Ability.OwnerEntity.Item;
            
            var handnessMultiplier = GetMultiplier(item.EquippedSlot);
            if (handnessMultiplier <= 0)
            {
                return 0;
            }

            var baseMultiplier = GetMultiplier(EquipmentSlot.GraspPrimaryMelee);
            var requiredMight = Item.Loader.Get(item.TemplateName)?.RequiredMight ?? 0;
            var mightDifference = activator.Being.Might * handnessMultiplier - requiredMight * baseMultiplier;
            var requiredFocus = Item.Loader.Get(item.TemplateName)?.RequiredFocus ?? 0;
            var focusDifference = activator.Being.Focus * handnessMultiplier - requiredFocus * baseMultiplier;
            var scale = 100
                        + ((requiredMight * (mightDifference + Math.Min(0, mightDifference)))
                         + (requiredFocus * (focusDifference + Math.Min(0, focusDifference)))) / handnessMultiplier;

            if (scale <= 0)
            {
                return 0;
            }

            return baseFactor * scale / 100;
        }

        private int GetMultiplier(EquipmentSlot slot)
        {
            var multiplier = 0;
            switch (slot)
            {
                case EquipmentSlot.GraspBothMelee:
                case EquipmentSlot.GraspBothRanged:
                    multiplier = 5;
                    break;
                case EquipmentSlot.GraspPrimaryMelee:
                case EquipmentSlot.GraspPrimaryRanged:
                    multiplier = 3;
                    break;
                case EquipmentSlot.GraspSecondaryMelee:
                case EquipmentSlot.GraspSecondaryRanged:
                    multiplier = 2;
                    break;
            }

            return multiplier;
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
                clone.Amount = Amount;
                clone.AmountExpression = AmountExpression;
                clone.EffectType = EffectType;
                clone.Function = Function;
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
            _durationAmount = default;
            _expirationTick = default;
            _expirationXp = default;
            _shouldTargetActivator = default;
            _amount = default;
            _amountExpression = default;
            _effectType = default;
            _function = default;
            _targetName = default;
            _targetEntityId = default;

            base.Clean();
        }
    }
}
