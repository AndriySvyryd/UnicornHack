namespace UnicornHack.Primitives;

/// <summary>
///     How the magnitudes of several contributions to the same dynamic property are combined when that
///     property is resolved. Contributions are folded in a fixed order (source-ability activation tier,
///     then the declared precedence of this function, then a stable id); the numeric order of the members
///     below is that precedence. This is the magnitude axis, orthogonal to how re-applications of the same
///     effect type combine over time.
///     Except for <see cref="Default" />, which is resolved before application, the order reflects precedence.
/// </summary>
public enum ValueCombinationFunction
{
    /// <summary>Uses the target property's declared default combination function.</summary>
    Default = 0,

    /// <summary>
    ///     Adds the contribution to the running value (booleans: logical OR). The common stacking modifier -
    ///     several sources each add their amount.
    /// </summary>
    Sum,

    /// <summary>
    ///     Subtracts the contribution from the running value. Damage effects use this to declare that their
    ///     positive magnitude lowers the resource; same-type damage magnitudes still aggregate additively.
    /// </summary>
    Subtract,

    /// <summary>
    ///     Scales the running value by the contribution read as a percentage (booleans: logical AND), so it
    ///     applies relative to what the lower-precedence contributions have already produced.
    /// </summary>
    Percent,

    /// <summary>The mean of the contributions at this tier, rounded up (booleans: majority vote, ties on).</summary>
    MeanRoundUp,

    /// <summary>The mean of the contributions at this tier, rounded down (booleans: majority vote, ties off).</summary>
    MeanRoundDown,

    /// <summary>Keeps the greater of the running value and the contribution (booleans: logical OR).</summary>
    Max,

    /// <summary>Keeps the lesser of the running value and the contribution (booleans: logical AND).</summary>
    Min,

    /// <summary>Replaces the running value with the contribution outright, ignoring what came before.</summary>
    Override,

    /// <summary>
    ///     Threshold accumulator measured against a fixed constant: contributions are summed into a banked
    ///     total that only takes effect once it reaches that constant (used by self-contained counters such
    ///     as overcharge).
    /// </summary>
    BuildUpFixed,

    /// <summary>
    ///     Threshold accumulator measured against the target's remaining health: the banked total takes
    ///     effect once it exceeds the target's current hit points (used by doom).
    /// </summary>
    BuildUpHealth,

    /// <summary>
    ///     Threshold accumulator owned by the strongest active source marker. Progress resets when a
    ///     different active source takes ownership, and the accumulated status only applies while a source
    ///     marker remains active.
    /// </summary>
    BuildUpPerSource
}

public static class ValueCombinationFunctionExtensions
{
    /// <summary>Whether the function is one of the threshold-accumulator (build-up) variants.</summary>
    public static bool IsBuildUp(this ValueCombinationFunction function)
        => function is ValueCombinationFunction.BuildUpFixed
            or ValueCombinationFunction.BuildUpHealth
            or ValueCombinationFunction.BuildUpPerSource;
}
