namespace UnicornHack.Primitives
{
    public enum ValueCombinationFunction
    {
        // The order reflects order of precedence
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
