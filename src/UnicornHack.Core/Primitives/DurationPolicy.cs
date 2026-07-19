namespace UnicornHack.Primitives;

/// <summary>
///     Decides whether re-applying the same <see cref="EffectType" /> to a target merges into the
///     existing active effect (a single instance whose timer combines per this policy) or stacks as an
///     independent instance. This is the duration/stacking axis; it is orthogonal to the magnitude axis,
///     which is governed by <see cref="ValueCombinationFunction" />.
/// </summary>
public enum DurationPolicy : byte
{
    /// <summary>
    ///     Each application is its own instance with its own expiry - true stacking. Used for
    ///     damage-over-time effects authored to pile up (N bleeds tick N times and expire separately).
    /// </summary>
    Independent,

    /// <summary>
    ///     A single instance; re-application sets the remaining duration to
    ///     <c>max(remaining, newDuration)</c>, leaving how the magnitudes combine to the
    ///     <see cref="ValueCombinationFunction" />.
    /// </summary>
    Refresh,

    /// <summary>
    ///     A single instance; re-application adds the new duration onto the remaining one, so the effect
    ///     lasts longer as it is reapplied.
    /// </summary>
    Extend,

    /// <summary>
    ///     A single instance; re-application resets the expiry to the new one.
    /// </summary>
    Replace
}
