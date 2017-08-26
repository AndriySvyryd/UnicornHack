namespace UnicornHack
{
    public enum ValueCombinationFunction
    {
        // The order reflects the function priority
        // Relative
        Sum = 0,
        Percent,

        // Absolute
        MeanRoundUp,
        MeanRoundDown,
        Max,
        Min,
        Override
    }
}