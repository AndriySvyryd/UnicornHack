namespace UnicornHack.Effects
{
    public class ChangedBoolProperty : ChangedProperty<bool>
    {
        public ChangedBoolProperty()
        {
        }

        public ChangedBoolProperty(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public override void Apply(Property<bool> property, ref (int RunningSum, int SummandCount) state)
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
                        state.RunningSum = property.TypedValue ? 1 : 0;
                        state.SummandCount = 1;
                        return;
                }
            }

            switch (Function)
            {
                case ValueCombinationFunction.Sum:
                    property.TypedValue |= Value;
                    break;
                case ValueCombinationFunction.Percent:
                    property.TypedValue &= Value;
                    break;
                case ValueCombinationFunction.Override:
                    property.TypedValue = Value;
                    break;
                case ValueCombinationFunction.Max:
                    property.TypedValue |= Value;
                    break;
                case ValueCombinationFunction.Min:
                    property.TypedValue &= Value;
                    break;
                case ValueCombinationFunction.MeanRoundDown:
                    state.RunningSum += Value ? 1 : 0;
                    state.SummandCount++;
                    property.TypedValue = state.RunningSum * 2 > state.SummandCount;
                    break;
                case ValueCombinationFunction.MeanRoundUp:
                    state.RunningSum += Value ? 1 : 0;
                    state.SummandCount++;
                    property.TypedValue = state.RunningSum * 2 >= state.SummandCount;
                    break;
            }

            state.RunningSum = property.TypedValue ? 1 : 0;
            state.SummandCount = 1;
        }
    }
}