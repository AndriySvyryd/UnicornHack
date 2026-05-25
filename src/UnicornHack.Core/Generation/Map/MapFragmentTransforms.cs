namespace UnicornHack.Generation.Map;

/// <summary>
///     Helpers for enumerating the symmetry orbit of a <see cref="MapFragment" /> based on
///     its declared transform-exclusion flags.
/// </summary>
public static class MapFragmentTransforms
{
    private static readonly MapFragmentTransform[] _full =
    [
        MapFragmentTransform.Identity,
        MapFragmentTransform.Rotate90,
        MapFragmentTransform.Rotate180,
        MapFragmentTransform.Rotate270,
        MapFragmentTransform.FlipHorizontal,
        MapFragmentTransform.FlipVertical,
        MapFragmentTransform.FlipDiagonal,
        MapFragmentTransform.FlipAntiDiagonal
    ];

    /// <summary>
    ///     Rotations only — used when the fragment has reflection symmetry
    ///     (<see cref="MapFragment.NoMirror" />).
    /// </summary>
    private static readonly MapFragmentTransform[] _rotationsOnly =
    [
        MapFragmentTransform.Identity,
        MapFragmentTransform.Rotate90,
        MapFragmentTransform.Rotate180,
        MapFragmentTransform.Rotate270
    ];

    /// <summary>
    ///     Identity + a single mirror — used when the fragment has full rotational symmetry
    ///     (<see cref="MapFragment.NoTurnOver" />).
    /// </summary>
    private static readonly MapFragmentTransform[] _identityAndMirror =
    [
        MapFragmentTransform.Identity,
        MapFragmentTransform.FlipHorizontal
    ];

    private static readonly MapFragmentTransform[] _identityOnly =
    [
        MapFragmentTransform.Identity
    ];

    /// <summary>
    ///     Returns the symmetry orbit (set of <see cref="MapFragmentTransform" />s that
    ///     should be materialized as distinct variants) for a fragment with the supplied
    ///     transform-exclusion flags.
    ///     <list type="bullet">
    ///         <item><c>noTransform</c> → 1 variant (identity).</item>
    ///         <item><c>noMirror &amp;&amp; noTurnOver</c> → 1 variant.</item>
    ///         <item><c>noTurnOver</c> → 2 variants (identity + one mirror).</item>
    ///         <item><c>noMirror</c> → 4 variants (identity + three rotations).</item>
    ///         <item>default → 8 variants (full D4).</item>
    ///     </list>
    /// </summary>
    public static ReadOnlySpan<MapFragmentTransform> GetOrbit(bool noMirror, bool noTurnOver, bool noTransform)
    {
        if (noTransform || (noMirror && noTurnOver))
        {
            return _identityOnly;
        }

        if (noTurnOver)
        {
            return _identityAndMirror;
        }

        if (noMirror)
        {
            return _rotationsOnly;
        }

        return _full;
    }

    /// <summary>
    ///     Returns the post-transform dimensions of a fragment of the given source size.
    ///     Width and height swap for the four transforms that rotate the diagonal axes
    ///     (90°, 270°, diagonal flip, anti-diagonal flip).
    /// </summary>
    public static (byte Width, byte Height) GetTransformedSize(this MapFragmentTransform transform, byte sourceWidth, byte sourceHeight)
        => transform switch
        {
            MapFragmentTransform.Rotate90 or MapFragmentTransform.Rotate270
                or MapFragmentTransform.FlipDiagonal or MapFragmentTransform.FlipAntiDiagonal
                => (sourceHeight, sourceWidth),
            _ => (sourceWidth, sourceHeight)
        };

    /// <summary>
    ///     Maps a point from the source fragment's coordinate space to the transformed
    ///     fragment's coordinate space. <paramref name="sourceWidth" /> and
    ///     <paramref name="sourceHeight" /> are the pre-transform dimensions.
    /// </summary>
    public static Point Apply(this MapFragmentTransform transform, Point point, byte sourceWidth, byte sourceHeight)
    {
        var x = point.X;
        var y = point.Y;
        return transform switch
        {
            MapFragmentTransform.Identity => point,
            MapFragmentTransform.Rotate90 => new Point((byte)(sourceHeight - 1 - y), x),
            MapFragmentTransform.Rotate180 => new Point((byte)(sourceWidth - 1 - x), (byte)(sourceHeight - 1 - y)),
            MapFragmentTransform.Rotate270 => new Point(y, (byte)(sourceWidth - 1 - x)),
            MapFragmentTransform.FlipHorizontal => new Point((byte)(sourceWidth - 1 - x), y),
            MapFragmentTransform.FlipVertical => new Point(x, (byte)(sourceHeight - 1 - y)),
            MapFragmentTransform.FlipDiagonal => new Point(y, x),
            MapFragmentTransform.FlipAntiDiagonal => new Point((byte)(sourceHeight - 1 - y), (byte)(sourceWidth - 1 - x)),
            _ => throw new ArgumentOutOfRangeException(nameof(transform), transform, null)
        };
    }
}
