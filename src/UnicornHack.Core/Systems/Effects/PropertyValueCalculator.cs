using System.Collections.Generic;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Effects
{
    public class PropertyValueCalculator
    {
        public void UpdatePropertyValue(
            PropertyDescription<int> propertyDescription,
            GameEntity targetEntity,
            EffectComponent effectComponent,
            GameManager manager)
        {
            var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
            if (targetComponent == null)
            {
                return;
            }

            int newValue;
            var runningSum = 0;
            var summandCount = 0;
            if (propertyDescription.IsCalculated)
            {
                var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, manager);
                newValue = propertyDescription.DefaultValue ?? default;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < activeEffects.Count; index++)
                {
                    var activeEffect = activeEffects[index];

                    Apply(activeEffect.Amount.Value, activeEffect.Function,
                        ref newValue, ref runningSum, ref summandCount);
                }

                propertyDescription.SetValue(newValue, targetComponent);
            }
            else
            {
                newValue = propertyDescription.GetValue(targetComponent);
                Apply(effectComponent.Amount.Value, effectComponent.Function,
                    ref newValue, ref runningSum, ref summandCount);
                propertyDescription.SetValue(newValue, targetComponent);
            }
        }

        public void UpdatePropertyValue(
            PropertyDescription<bool> propertyDescription,
            GameEntity targetEntity,
            EffectComponent effectComponent,
            GameManager manager)
        {
            bool newValue;
            var runningSum = 0;
            var summandCount = 0;
            var targetComponent = targetEntity.FindComponent(propertyDescription.ComponentId);
            if (propertyDescription.IsCalculated)
            {
                var activeEffects = GetSortedAppliedEffects(targetEntity, propertyDescription, manager);
                newValue = propertyDescription.DefaultValue ?? default;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < activeEffects.Count; index++)
                {
                    var activeEffect = activeEffects[index];

                    Apply(activeEffect.Amount.Value != 0, activeEffect.Function,
                        ref newValue, ref runningSum, ref summandCount);
                }

                propertyDescription.SetValue(newValue, targetComponent);
            }
            else
            {
                newValue = propertyDescription.GetValue(targetComponent);
                Apply(effectComponent.Amount.Value != 0, effectComponent.Function,
                    ref newValue, ref runningSum, ref summandCount);
                propertyDescription.SetValue(newValue, targetComponent);
            }
        }

        private void Apply(
            int amount,
            ValueCombinationFunction function,
            ref int propertyValue,
            ref int runningSum,
            ref int summandCount)
        {
            if (summandCount == 0)
            {
                switch (function)
                {
                    case ValueCombinationFunction.Sum:
                    case ValueCombinationFunction.Percent:
                        break;
                    default:
                        propertyValue = amount;
                        runningSum = propertyValue;
                        summandCount = 1;
                        return;
                }
            }

            switch (function)
            {
                case ValueCombinationFunction.Sum:
                    propertyValue += amount;
                    break;
                case ValueCombinationFunction.Percent:
                    propertyValue = (propertyValue * amount) / 100;
                    break;
                case ValueCombinationFunction.Override:
                    propertyValue = amount;
                    break;
                case ValueCombinationFunction.Max:
                    propertyValue = propertyValue > amount ? propertyValue : amount;
                    break;
                case ValueCombinationFunction.Min:
                    propertyValue = propertyValue < amount ? propertyValue : amount;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    runningSum += amount;
                    summandCount++;
                    propertyValue = runningSum / summandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    runningSum += amount;
                    summandCount++;
                    propertyValue = (runningSum + summandCount - 1) / summandCount;
                    break;
            }

            runningSum = propertyValue;
            summandCount = 1;
        }

        private void Apply(
            bool amount,
            ValueCombinationFunction function,
            ref bool propertyValue,
            ref int runningSum,
            ref int summandCount)
        {
            if (summandCount == 0)
            {
                switch (function)
                {
                    case ValueCombinationFunction.Sum:
                    case ValueCombinationFunction.Percent:
                        break;
                    default:
                        propertyValue = amount;
                        runningSum = propertyValue ? 1 : 0;
                        summandCount = 1;
                        return;
                }
            }

            switch (function)
            {
                case ValueCombinationFunction.Sum:
                    propertyValue |= amount;
                    break;
                case ValueCombinationFunction.Percent:
                    propertyValue &= amount;
                    break;
                case ValueCombinationFunction.Override:
                    propertyValue = amount;
                    break;
                case ValueCombinationFunction.Max:
                    propertyValue |= amount;
                    break;
                case ValueCombinationFunction.Min:
                    propertyValue &= amount;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    runningSum += amount ? 1 : 0;
                    summandCount++;
                    propertyValue = runningSum * 2 > summandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    runningSum += amount ? 1 : 0;
                    summandCount++;
                    propertyValue = runningSum * 2 >= summandCount;
                    break;
            }

            runningSum = propertyValue ? 1 : 0;
            summandCount = 1;
        }

        private List<EffectComponent> GetSortedAppliedEffects(
            GameEntity targetEntity, PropertyDescription propertyDescription, GameManager manager)
        {
            var activeEffects = new List<EffectComponent>();
            foreach (var otherEffectEntity in manager.AppliedEffectsToAffectableEntityRelationship[targetEntity.Id])
            {
                var otherEffectComponent = (EffectComponent)
                    otherEffectEntity.FindComponent((int)EntityComponent.Effect);
                if (otherEffectComponent.EffectType == EffectType.ChangeProperty
                    && otherEffectComponent.PropertyName.Equals(propertyDescription.Name))
                {
                    activeEffects.Add(otherEffectComponent);
                }
            }

            activeEffects.Sort(AppliedEffectComponentComparer.Instance);
            return activeEffects;
        }

        private class AppliedEffectComponentComparer : IComparer<EffectComponent>
        {
            public static readonly AppliedEffectComponentComparer Instance = new AppliedEffectComponentComparer();

            private AppliedEffectComponentComparer()
            {
            }

            public int Compare(EffectComponent x, EffectComponent y)
            {
                var game = x.Entity.Manager;
                var xAbility = x.SourceAbilityId.HasValue
                    ? game.FindEntity(x.SourceAbilityId.Value).Ability
                    : null;
                var yAbility = y.SourceAbilityId.HasValue
                    ? game.FindEntity(y.SourceAbilityId.Value).Ability
                    : null;

                if (xAbility != null
                    && yAbility != null)
                {
                    switch (xAbility.Activation)
                    {
                        case ActivationType.Always:
                            switch (yAbility.Activation)
                            {
                                case ActivationType.Always:
                                    if (xAbility.Name == LivingSystem.InnateAbilityName)
                                    {
                                        if (yAbility.Name != LivingSystem.InnateAbilityName)
                                        {
                                            return -1;
                                        }
                                    }
                                    else if (yAbility.Name == LivingSystem.InnateAbilityName)
                                    {
                                        return 1;
                                    }

                                    break;
                                default:
                                    return -1;
                            }

                            break;
                        case ActivationType.WhileToggled:
                            switch (yAbility.Activation)
                            {
                                case ActivationType.Always:
                                    return 1;
                                case ActivationType.WhileToggled:
                                    break;
                                default:
                                    return -1;
                            }

                            break;
                        case ActivationType.WhilePossessed:
                            switch (yAbility.Activation)
                            {
                                case ActivationType.Always:
                                case ActivationType.WhileToggled:
                                    return 1;
                                case ActivationType.WhilePossessed:
                                    break;
                                default:
                                    return -1;
                            }

                            break;
                        case ActivationType.WhileEquipped:
                            switch (yAbility.Activation)
                            {
                                case ActivationType.Always:
                                case ActivationType.WhileToggled:
                                case ActivationType.WhilePossessed:
                                    return 1;
                                case ActivationType.WhileEquipped:
                                    break;
                                default:
                                    return -1;
                            }

                            break;
                        default:
                            switch (yAbility.Activation)
                            {
                                case ActivationType.Always:
                                case ActivationType.WhileToggled:
                                case ActivationType.WhilePossessed:
                                case ActivationType.WhileEquipped:
                                    return 1;
                            }

                            break;
                    }
                }

                var result = y.Function - x.Function;
                if (result != 0)
                {
                    return result;
                }

                return x.EntityId - y.EntityId;
            }
        }
    }
}
