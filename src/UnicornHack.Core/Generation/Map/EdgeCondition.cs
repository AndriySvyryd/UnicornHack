namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-tile placement constraint that a single perimeter cell of a static map fragment
///     imposes on its neighbour at placement time. Authored via condition symbols inside a
///     <c>~</c>-marked conditional row or column on the fragment's <see cref="MapFragment.Map" />.
///     Combined into <see cref="MapFragment.NorthEdge" /> / <see cref="MapFragment.SouthEdge" /> /
///     <see cref="MapFragment.EastEdge" /> / <see cref="MapFragment.WestEdge" /> arrays whose
///     length matches <see cref="MapFragment.PayloadArea" />.
///     <para>
///     Tile-specific constraints share their numeric value with the matching
///     <see cref="MapFeature" /> so a direct cast converts between the two. Non-tile
///     conditions (<see cref="Filled" />, <see cref="Unfilled" />, <see cref="Edge" />,
///     <see cref="Doorway" />) live in the 200+ range so they never collide with a
///     <see cref="MapFeature" /> value.
///     </para>
/// </summary>
public enum EdgeConditionKind : byte
{
    /// <summary>No constraint. Authored as space; the implicit value for
    /// <c>~</c>-marked rows/cols that contain no other symbols. Numerically aligned with
    /// <see cref="MapFeature.Default" /> but evaluated as "no constraint" rather than as a
    /// tile match.</summary>
    Default = MapFeature.Default,

    // --- Tile-specific conditions: neighbour must equal the named MapFeature. Numeric
    //     values match MapFeature so a direct cast converts between the two.
    RockFloor = MapFeature.RockFloor,
    StoneFloor = MapFeature.StoneFloor,
    RockWall = MapFeature.RockWall,
    StoneWall = MapFeature.StoneWall,
    StoneArchway = MapFeature.StoneArchway,
    DeityAltar = MapFeature.DeityAltar,
    MajorShrine = MapFeature.MajorShrine,
    MinorShrine = MapFeature.MinorShrine,
    RitualShrine = MapFeature.RitualShrine,
    SmallFountain = MapFeature.SmallFountain,
    BigFountain = MapFeature.BigFountain,
    SkillPedestal = MapFeature.SkillPedestal,
    RacePedestal = MapFeature.RacePedestal,
    Pool = MapFeature.Pool,
    TraitPedestal = MapFeature.TraitPedestal,
    FeatPedestal = MapFeature.FeatPedestal,
    ArmorStand = MapFeature.ArmorStand,
    ItemCase = MapFeature.ItemCase,
    WeaponStand = MapFeature.WeaponStand,

    // --- Non-feature conditions: values above 200 so they never collide with any present
    //     or future MapFeature numeric value.

    /// <summary>Neighbour must be any non-outside tile (anything already placed).
    /// Authored as <c>F</c>.</summary>
    Filled = 202,

    /// <summary>Neighbour is either still-empty space inside the room or outside the room
    /// altogether (i.e. the room's own edge). Authored as <c>U</c>.</summary>
    Unfilled = 203,

    /// <summary>This fragment's payload edge tile must coincide with the room's edge —
    /// the corresponding neighbour cell lies outside the room. Authored as <c>E</c>.</summary>
    Edge = 204,

    /// <summary>Neighbour must be a walkable threshold tile — either
    /// <see cref="MapFeature.StoneFloor" /> or <see cref="MapFeature.StoneArchway" />.
    /// Authored as <c>+</c>: mirrors the doorway-glyph semantic so an authored
    /// payload-edge <c>+</c> aligns with a floor or archway on the other side.</summary>
    Doorway = 205
}
