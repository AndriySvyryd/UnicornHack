namespace UnicornHack.Generation.Map;

/// <summary>
///     One of the eight elements of the dihedral group D4 (symmetries of a square). Applied
///     to a <see cref="MapFragment" />'s payload during variant materialization to produce
///     a rotated and/or reflected copy.
///     <para>
///         The four "rotate" entries are pure rotations of the source; the four "flip"
///         entries are an axis reflection. <see cref="FlipDiagonal" /> mirrors across the
///         top-left / bottom-right axis and is equivalent to <c>Rotate90</c> composed with
///         <see cref="FlipHorizontal" />; <see cref="FlipAntiDiagonal" /> mirrors across the
///         top-right / bottom-left axis and equals <c>Rotate270</c> composed with
///         <see cref="FlipHorizontal" />.
///     </para>
/// </summary>
public enum MapFragmentTransform
{
    Identity = 0,
    Rotate90 = 1,
    Rotate180 = 2,
    Rotate270 = 3,
    FlipHorizontal = 4,
    FlipVertical = 5,
    FlipDiagonal = 6,
    FlipAntiDiagonal = 7
}
