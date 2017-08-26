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

        public override void Apply(Property<int> property, ref (int RunningSum, int SummandCount) state)
        {
            if (state.SummandCount == 0)
            {
                switch (Function)
                {
                    case ValueCombinationFunction.Sum:
                    case ValueCombinationFunction.Percent:
                        break;
                    default:
                        property.TypedValue = Value;
                        state.RunningSum = property.TypedValue;
                        state.SummandCount = 1;
                        return;
                }
            }

            switch (Function)
            {
                case ValueCombinationFunction.Sum:
                    property.TypedValue += Value;
                    break;
                case ValueCombinationFunction.Percent:
                    property.TypedValue = (property.TypedValue * Value) / 100;
                    break;
                case ValueCombinationFunction.Override:
                    property.TypedValue = Value;
                    break;
                case ValueCombinationFunction.Max:
                    property.TypedValue = property.TypedValue > Value ? property.TypedValue : Value;
                    break;
                case ValueCombinationFunction.Min:
                    property.TypedValue = property.TypedValue < Value ? property.TypedValue : Value;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    state.RunningSum += Value;
                    state.SummandCount++;
                    property.TypedValue = state.RunningSum / state.SummandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    state.RunningSum += Value;
                    state.SummandCount++;
                    property.TypedValue = (state.RunningSum + state.SummandCount - 1) / state.SummandCount;
                    break;
            }

            state.RunningSum = property.TypedValue;
            state.SummandCount = 1;
        }
    }
}