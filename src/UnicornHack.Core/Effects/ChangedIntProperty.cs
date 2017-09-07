namespace UnicornHack.Effects
{
    public class ChangedIntProperty : ChangedProperty<int>
    {
        public ChangedIntProperty()
        {
        }

        public ChangedIntProperty(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public override void Apply(CalculatedProperty<int> property, ref (int RunningSum, int SummandCount) state)
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
                        state.RunningSum = property.LastValue;
                        state.SummandCount = 1;
                        return;
                }
            }

            switch (Function)
            {
                case ValueCombinationFunction.Sum:
                    property.LastValue += Value;
                    break;
                case ValueCombinationFunction.Percent:
                    property.LastValue = (property.LastValue * Value) / 100;
                    break;
                case ValueCombinationFunction.Override:
                    property.LastValue = Value;
                    break;
                case ValueCombinationFunction.Max:
                    property.LastValue = property.LastValue > Value ? property.LastValue : Value;
                    break;
                case ValueCombinationFunction.Min:
                    property.LastValue = property.LastValue < Value ? property.LastValue : Value;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    state.RunningSum += Value;
                    state.SummandCount++;
                    property.LastValue = state.RunningSum / state.SummandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    state.RunningSum += Value;
                    state.SummandCount++;
                    property.LastValue = (state.RunningSum + state.SummandCount - 1) / state.SummandCount;
                    break;
            }

            state.RunningSum = property.LastValue;
            state.SummandCount = 1;
        }
    }
}