namespace UnicornHack.Systems.Effects;

/// <summary>
///     The value-combination core shared by dynamic-property resolution. Folds an ordered sequence of
///     modifier contributions onto a base value, preserving exactly the semantics of the legacy
///     property engine (`EffectApplicationSystem.ApplyOperation`) and its contribution ordering
///     (`AppliedEffectComponentComparer`): contributions sort by source-ability activation tier, then
///     by <see cref="ValueCombinationFunction" /> precedence, then by a stable id.
/// </summary>
public static class PropertyValueCombiner
{
    /// <summary>
    ///     Applies a single contribution to the running <paramref name="value" />. The caller threads
    ///     <paramref name="runningSum" /> and <paramref name="summandCount" /> across the fold; when
    ///     starting from an established base they begin at <c>(base, 1)</c>, otherwise at
    ///     <c>(default, 0)</c>.
    /// </summary>
    public static void Apply(
        int amount,
        ValueCombinationFunction function,
        ref int value,
        ref int runningSum,
        ref int summandCount)
    {
        if (summandCount == 0)
        {
            switch (function)
            {
                case ValueCombinationFunction.Sum:
                case ValueCombinationFunction.Subtract:
                case ValueCombinationFunction.Percent:
                case ValueCombinationFunction.BuildUpFixed:
                case ValueCombinationFunction.BuildUpHealth:
                case ValueCombinationFunction.BuildUpPerSource:
                    break;
                default:
                    value = amount;
                    runningSum = value;
                    summandCount = 1;
                    return;
            }
        }

        switch (function)
        {
            case ValueCombinationFunction.Sum:
            case ValueCombinationFunction.BuildUpFixed:
            case ValueCombinationFunction.BuildUpHealth:
            case ValueCombinationFunction.BuildUpPerSource:
                value += amount;
                break;
            case ValueCombinationFunction.Subtract:
                value -= amount;
                break;
            case ValueCombinationFunction.Percent:
                value = value * amount / 100;
                break;
            case ValueCombinationFunction.Override:
                value = amount;
                break;
            case ValueCombinationFunction.Max:
                value = value > amount ? value : amount;
                break;
            case ValueCombinationFunction.Min:
                value = value < amount ? value : amount;
                break;
            case ValueCombinationFunction.MeanRoundDown:
                runningSum += amount;
                summandCount++;
                value = runningSum / summandCount;
                break;
            case ValueCombinationFunction.MeanRoundUp:
                runningSum += amount;
                summandCount++;
                value = (runningSum + summandCount - 1) / summandCount;
                break;
        }

        runningSum = value;
        summandCount = 1;
    }

    /// <summary>
    ///     The boolean analogue of <see cref="Apply(int, ValueCombinationFunction, ref int, ref int, ref int)" />,
    ///     matching the legacy engine's boolean <c>Apply</c>: <c>Sum</c>/<c>Max</c> are OR, <c>Percent</c>/
    ///     <c>Min</c> are AND, <c>Override</c> replaces, and the means use a majority vote.
    /// </summary>
    public static void ApplyBool(
        bool amount,
        ValueCombinationFunction function,
        ref bool value,
        ref int runningSum,
        ref int summandCount)
    {
        if (summandCount == 0)
        {
            switch (function)
            {
                case ValueCombinationFunction.Sum:
                case ValueCombinationFunction.Subtract:
                case ValueCombinationFunction.Percent:
                    break;
                default:
                    value = amount;
                    runningSum = value ? 1 : 0;
                    summandCount = 1;
                    return;
            }
        }

        switch (function)
        {
            case ValueCombinationFunction.Sum:
                value |= amount;
                break;
            case ValueCombinationFunction.Subtract:
                value &= !amount;
                break;
            case ValueCombinationFunction.Percent:
                value &= amount;
                break;
            case ValueCombinationFunction.Override:
                value = amount;
                break;
            case ValueCombinationFunction.Max:
                value |= amount;
                break;
            case ValueCombinationFunction.Min:
                value &= amount;
                break;
            case ValueCombinationFunction.MeanRoundDown:
                runningSum += amount ? 1 : 0;
                summandCount++;
                value = runningSum * 2 > summandCount;
                break;
            case ValueCombinationFunction.MeanRoundUp:
                runningSum += amount ? 1 : 0;
                summandCount++;
                value = runningSum * 2 >= summandCount;
                break;
        }

        runningSum = value ? 1 : 0;
        summandCount = 1;
    }

    /// <summary>
    ///     The layer order of a contribution by its source ability's activation, lowest applying
    ///     first: innate/always, then toggled, possessed, equipped, then everything else. Mirrors
    ///     the tiers in the legacy <c>AppliedEffectComponentComparer</c>.
    /// </summary>
    public static int GetActivationOrder(ActivationType activation)
    {
        if ((activation & ActivationType.Always) != 0)
        {
            return 0;
        }

        if ((activation & ActivationType.WhileToggled) != 0)
        {
            return 1;
        }

        if ((activation & ActivationType.WhilePossessed) != 0)
        {
            return 2;
        }

        if ((activation & ActivationType.WhileEquipped) != 0)
        {
            return 3;
        }

        return 4;
    }
}
