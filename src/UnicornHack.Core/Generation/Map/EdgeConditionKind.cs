namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-tile placement constraint that a single perimeter cell of a static map fragment
///     imposes on its neighbor at placement time. Authored via condition symbols inside a
///     <c>~</c>-marked conditional row or column on the fragment's <see cref="MapFragment.Map" />.
///     Combined into <see cref="MapFragment.NorthEdge" /> / <see cref="MapFragment.SouthEdge" /> /
///     <see cref="MapFragment.EastEdge" /> / <see cref="MapFragment.WestEdge" /> arrays whose
///     length matches <see cref="MapFragment.PayloadArea" />.
///     <para>
///     Tile-specific constraints share their numeric value with the matching
///     <see cref="MapFeature" /> so a direct cast converts between the two. Non-tile
///     conditions (<see cref="Filled" />, <see cref="Unfilled" />, <see cref="Edge" />,
///     <see cref="Doorway" />) follow immediately above the highest tile-specific value so
///     the full enum range stays compact (the per-value array cache in
///     <see cref="MapFragment" /> is sized to it). They must stay above every
///     <see cref="MapFeature" /> that can appear as a tile-specific condition; adding a
///     glyph-mappable <see cref="MapFeature" /> with a higher numeric value requires
///     renumbering these four to stay above it.
///     </para>
/// </summary>
public enum EdgeConditionKind : byte
{
    /// <summary>No constraint. Authored as space; the implicit value for
    /// <c>~</c>-marked rows/cols that contain no other symbols. Numerically aligned with
    /// <see cref="MapFeature.Default" /> but evaluated as "no constraint" rather than as a
    /// tile match.</summary>
    Default = MapFeature.Default,

    // --- Tile-specific conditions: neighbor must equal the named MapFeature. Numeric
    //     values match MapFeature so a direct cast converts between the two.
    RockFloor = MapFeature.RockFloor,
    StoneFloor = MapFeature.StoneFloor,
    RockWall = MapFeature.RockWall,
    StoneWall = MapFeature.StoneWall,
    StoneArchway = MapFeature.StoneArchway,
    Pool = MapFeature.Pool,

    // --- Non-feature conditions: values immediately above the highest concrete MapFeature.

    /// <summary>Neighbor must be any non-outside tile (anything already placed).
    /// Authored as <c>F</c>.</summary>
    Filled = MapFeature.InactiveGildedWeaponStand + 1,

    /// <summary>Neighbor is either still-empty space inside the room or outside the room
    /// altogether (i.e. the room's own edge). Authored as <c>U</c>.</summary>
    Unfilled,

    /// <summary>This fragment's payload edge tile must coincide with the room's edge —
    /// the corresponding neighbor cell lies outside the room. Authored as <c>E</c>.</summary>
    Edge,

    /// <summary>Neighbor must be a walkable threshold tile — either
    /// <see cref="MapFeature.StoneFloor" /> or <see cref="MapFeature.StoneArchway" />.
    /// Authored as <c>+</c>: mirrors the doorway-glyph semantic so an authored
    /// payload-edge <c>+</c> aligns with a floor or archway on the other side.</summary>
    Doorway
}
