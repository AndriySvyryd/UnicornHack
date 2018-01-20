using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class ChangedBoolProperty : ChangedProperty<bool>
    {
        public ChangedBoolProperty()
        {
        }

        public ChangedBoolProperty(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public override void Apply(CalculatedProperty<bool> property, ref (int RunningSum, int SummandCount) state)
        {
            if (state.SummandCount == 0)
            {
                switch (Function)
                {
                    case ValueCombinationFunction.Sum:
                    case ValueCombinationFunction.Percent:
                        break;
                    default:
                        property.LastValue = Value;
                        state.RunningSum = property.LastValue ? 1 : 0;
                        state.SummandCount = 1;
                        return;
                }
            }

            switch (Function)
            {
                case ValueCombinationFunction.Sum:
                    property.LastValue |= Value;
                    break;
                case ValueCombinationFunction.Percent:
                    property.LastValue &= Value;
                    break;
                case ValueCombinationFunction.Override:
                    property.LastValue = Value;
                    break;
                case ValueCombinationFunction.Max:
                    property.LastValue |= Value;
                    break;
                case ValueCombinationFunction.Min:
                    property.LastValue &= Value;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    state.RunningSum += Value ? 1 : 0;
                    state.SummandCount++;
                    property.LastValue = state.RunningSum * 2 > state.SummandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    state.RunningSum += Value ? 1 : 0;
                    state.SummandCount++;
                    property.LastValue = state.RunningSum * 2 >= state.SummandCount;
                    break;
            }

            state.RunningSum = property.LastValue ? 1 : 0;
            state.SummandCount = 1;
        }
    }
}