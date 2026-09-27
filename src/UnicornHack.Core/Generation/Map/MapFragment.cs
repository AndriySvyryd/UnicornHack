using System.Buffers;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation.Map;

public abstract class MapFragment : ICSScriptSerializable, ILoadable
{
    /// <summary>Glyph reserved as a wildcard feature slot. Binds to any placeable
    /// <see cref="Systems.Levels.RoomFeatures" /> the room demands but hasn't placed yet.</summary>
    public const char FeatureWildcardGlyph = '$';

    /// <summary>Glyph reserved for an authored doorway candidate. Marks a payload-perimeter
    /// tile that the layout may carve into a door when wiring the level graph.</summary>
    public const char DoorwayGlyph = '+';

    /// <summary>Glyph reserved for a trap candidate. Recorded on the chunk via
    /// <see cref="Chunk.TrapPoints" /> for a later materialization pass.</summary>
    public const char TrapGlyph = '^';

    /// <summary>Glyph for a pool tile inside the payload. At the four corners the same
    /// glyph instead marks adjacent edge rows/columns as conditional — disambiguated by
    /// position during parsing.</summary>
    public const char PoolGlyph = '~';

    /// <summary>Glyph reserved for "place any difficulty-appropriate creature here" at
    /// creature-generation time. Tracked separately from <see cref="FeatureGlyphs" /> on the
    /// chunk via <see cref="Chunk.CreaturePoints" />.</summary>
    public const char CreatureWildcardGlyph = '%';

    /// <summary>
    ///     Canonical glyph - <see cref="Systems.Levels.RoomFeatures" /> table. Authored on a
    ///     fragment's <see cref="Map" /> payload exactly like any other tile glyph; the
    ///     underlying tile resolves to <see cref="Primitives.MapFeature.StoneFloor" /> and the
    ///     <see cref="Chunk.FeaturePoints" /> entry records which feature should be realized
    ///     there at room finalization. A <see langword="null" /> value (only for
    ///     <see cref="FeatureWildcardGlyph" />) means the binding pass picks one of the
    ///     room's outstanding placeable features at random.
    ///     <para>
    ///         Feature glyphs are <strong>payload-only</strong>. The conditional edge parser
    ///         (<see cref="ParseEdgeConditions" />) only accepts the edge-condition
    ///         vocabulary (<c>~</c>, <c>.</c>, space, <c>S</c>, <c>F</c>, <c>U</c>, <c>E</c>,
    ///         and the terrain glyphs <c>#</c>, <c>,</c>, <c>A</c>) and throws on every
    ///         feature glyph by design &#8212; feature glyphs describe room content, not neighbor
    ///         match conditions.
    ///     </para>
    /// </summary>
    public static readonly FrozenDictionary<char, RoomFeatures?> FeatureGlyphs =
        new Dictionary<char, RoomFeatures?>
        {
            ['_'] = RoomFeatures.DeityAltar,
            ['\\'] = RoomFeatures.MajorShrine,
            ['/'] = RoomFeatures.MinorShrine,
            ['&'] = RoomFeatures.RitualShrine,
            ['{'] = RoomFeatures.SmallFountain,
            ['}'] = RoomFeatures.BigFountain,
            ['?'] = RoomFeatures.SkillPedestal,
            ['!'] = RoomFeatures.RacePedestal,
            ['"'] = RoomFeatures.TraitPedestal,
            ['='] = RoomFeatures.FeatPedestal,
            ['['] = RoomFeatures.ArmorStand,
            [']'] = RoomFeatures.GildedArmorStand,
            ['('] = RoomFeatures.ItemCase,
            [':'] = RoomFeatures.GildedItemCase,
            [')'] = RoomFeatures.WeaponStand,
            [';'] = RoomFeatures.GildedWeaponStand,
            [TrapGlyph] = RoomFeatures.Trapped,
            [FeatureWildcardGlyph] = null
        }.ToFrozenDictionary();

    /// <summary>
    ///     Maps each placeable <see cref="RoomFeatures" /> bit to the <see cref="MapFeature" />
    ///     that <see cref="Write" /> commits to the terrain when the matching feature glyph
    ///     is encountered (or that <see cref="BindWildcardFeatures" /> commits after a
    ///     <see cref="FeatureWildcardGlyph" /> is bound). Layout / sensing code intentionally
    ///     does <em>not</em> register these bits as walkable or visibility-blocking — the
    ///     feature acts purely as a tile-level identity tag on top of the underlying floor.
    /// </summary>
    public static readonly FrozenDictionary<RoomFeatures, MapFeature> RoomFeatureToMapFeature =
        new Dictionary<RoomFeatures, MapFeature>
        {
            [RoomFeatures.DeityAltar] = MapFeature.DeityAltar,
            [RoomFeatures.MajorShrine] = MapFeature.MajorShrine,
            [RoomFeatures.MinorShrine] = MapFeature.MinorShrine,
            [RoomFeatures.RitualShrine] = MapFeature.RitualShrine,
            [RoomFeatures.SmallFountain] = MapFeature.SmallFountain,
            [RoomFeatures.BigFountain] = MapFeature.BigFountain,
            [RoomFeatures.SkillPedestal] = MapFeature.SkillPedestal,
            [RoomFeatures.RacePedestal] = MapFeature.RacePedestal,
            [RoomFeatures.TraitPedestal] = MapFeature.TraitPedestal,
            [RoomFeatures.FeatPedestal] = MapFeature.FeatPedestal,
            [RoomFeatures.ArmorStand] = MapFeature.ArmorStand,
            [RoomFeatures.GildedArmorStand] = MapFeature.GildedArmorStand,
            [RoomFeatures.ItemCase] = MapFeature.ItemCase,
            [RoomFeatures.GildedItemCase] = MapFeature.GildedItemCase,
            [RoomFeatures.WeaponStand] = MapFeature.WeaponStand,
            [RoomFeatures.GildedWeaponStand] = MapFeature.GildedWeaponStand
        }.ToFrozenDictionary();

    /// <summary>
    ///     The non-placeable subset of <see cref="RoomFeatures" /> — features that describe
    ///     the whole room rather than a single tile (Secret / Cursed / Challenge / Boss).
    ///     Authors gate fragments into these rooms via positive feature tags
    ///     (e.g. <c>cursed</c>) and out of them via <c>!{feature}</c> opt-out tags; the
    ///     demand-aware bonus in <see cref="GetDemandMultiplier" /> favours matching tags
    ///     and penalises matching anti-tags exactly like the placeable bits.
    /// </summary>
    public const RoomFeatures NonPlaceableFeatures =
        RoomFeatures.Secret | RoomFeatures.Cursed | RoomFeatures.Challenge | RoomFeatures.Boss;

    /// <summary>
    ///     All <see cref="RoomFeatures" /> bits that can be realized on a single tile —
    ///     the value range of <see cref="FeatureGlyphs" /> (excluding the wildcard's
    ///     <see langword="null" />, and excluding the layout-only bits
    ///     <see cref="RoomFeatures.Entrance" /> / <see cref="RoomFeatures.Exit" /> which
    ///     are placed by the level layout via stairs glyphs, not by feature glyphs).
    /// </summary>
    public static readonly RoomFeatures[] PlaceableFeatureBits =
        FeatureGlyphs.Values.Where(v => v.HasValue).Select(v => v!.Value).Distinct().ToArray();

    /// <summary>
    ///     Bitwise-OR of every bit in <see cref="PlaceableFeatureBits" />. Convenient for
    ///     masking a <see cref="RoomFeatures" /> value down to just the bits a fragment can
    ///     satisfy via <see cref="FeatureGlyphs" />.
    /// </summary>
    public static readonly RoomFeatures PlaceableFeaturesMask =
        PlaceableFeatureBits.Aggregate((RoomFeatures)0, (mask, bit) => mask | bit);

    /// <summary>
    ///     Canonical kebab-case tag name for every <see cref="RoomFeatures" /> bit a room
    ///     can demand from a fragment. Authors expose a fragment's capabilities by adding
    ///     these tags to <see cref="Tags" /> (or opting out via the <c>!</c>-prefix /
    ///     <see cref="AntiTags" /> mechanism). The auto-tag pass adds the matching entry
    ///     for every feature glyph it sees in the payload; the demand-aware weight bonus
    ///     in <see cref="GetDemandMultiplier" /> looks up tag names through this table.
    ///     <para>
    ///         <see cref="RoomFeatures.Entrance" /> and <see cref="RoomFeatures.Exit" /> map
    ///         to <c>entrance</c> and <c>exit</c> respectively — the connection
    ///         glyphs <c>&lt;</c> / <c>&gt;</c> emit those tags directly from the
    ///         auto-tag pass in <see cref="ApplyTagDerivations" />, and a fragment with a
    ///         dynamic stairs provider can author them explicitly.
    ///     </para>
    /// </summary>
    public static readonly FrozenDictionary<RoomFeatures, string> FeatureTagNames =
        new Dictionary<RoomFeatures, string>
        {
            [RoomFeatures.Entrance] = "entrance",
            [RoomFeatures.Exit] = "exit",
            [RoomFeatures.SkillPedestal] = "skill-pedestal",
            [RoomFeatures.TraitPedestal] = "trait-pedestal",
            [RoomFeatures.FeatPedestal] = "feat-pedestal",
            [RoomFeatures.RacePedestal] = "race-pedestal",
            [RoomFeatures.DeityAltar] = "deity-altar",
            [RoomFeatures.ArmorStand] = "armor-stand",
            [RoomFeatures.GildedArmorStand] = "gilded-armor-stand",
            [RoomFeatures.ItemCase] = "item-case",
            [RoomFeatures.GildedItemCase] = "gilded-item-case",
            [RoomFeatures.WeaponStand] = "weapon-stand",
            [RoomFeatures.GildedWeaponStand] = "gilded-weapon-stand",
            [RoomFeatures.SmallFountain] = "small-fountain",
            [RoomFeatures.BigFountain] = "big-fountain",
            [RoomFeatures.MinorShrine] = "minor-shrine",
            [RoomFeatures.MajorShrine] = "major-shrine",
            [RoomFeatures.RitualShrine] = "ritual-shrine",
            [RoomFeatures.Trapped] = "trapped",
            [RoomFeatures.Secret] = "secret",
            [RoomFeatures.Cursed] = "cursed",
            [RoomFeatures.Challenge] = "challenge",
            [RoomFeatures.Boss] = "boss",
            [RoomFeatures.Shortcut] = "shortcut"
        }.ToFrozenDictionary();

    /// <summary>
    ///     Tag added by the auto-derivation pass for the feature wildcard glyph
    ///     <see cref="FeatureWildcardGlyph" />. A fragment carrying this tag can satisfy
    ///     any outstanding placeable demand because its wildcard cells bind at placement
    ///     time through <see cref="BindWildcardFeatures" />.
    /// </summary>
    public const string WildcardFeatureTag = "wildcard-feature";

    /// <summary>
    ///     Per-direction door tag emitted by <see cref="ApplyTagDerivations" /> when
    ///     an authored <c>+</c> doorway sits on the corresponding payload edge. Used by
    ///     the demand-aware bonus to favour fragments whose authored doorways line up with
    ///     the lot's demanded intra-level connection directions. Only cardinal directions
    ///     have an entry.
    /// </summary>
    public static readonly FrozenDictionary<Direction, string> DoorDirectionTag =
        new Dictionary<Direction, string>
        {
            [Direction.North] = "door-n",
            [Direction.South] = "door-s",
            [Direction.East] = "door-e",
            [Direction.West] = "door-w"
        }.ToFrozenDictionary();

    /// <summary>
    ///     Single source of truth for the payload glyph, <see cref="MapFeature" />
    ///     mapping. Used by <see cref="Write" /> when committing a tile and by
    ///     <see cref="EvaluatePayloadOverlapCompatibility" /> to evaluate shifted-overlap
    ///     compatibility against placed neighbors. Glyphs that don't carry a tile (space)
    ///     return <see cref="MapFeature.Default" />. The default branch covers <c>'.'</c>,
    ///     <c>'+'</c> (doorway candidate), <c>'&lt;'</c>/<c>'&gt;'</c> (connections),
    ///     <c>'%'</c> (creature wildcard), <c>'$'</c> (feature wildcard pre-binding),
    ///     <c>'^'</c> (trap), and any annotated glyph - all of which sit on plain
    ///     stone floor.
    /// </summary>
    public static MapFeature GlyphToFeature(char c)
        => TryGlyphToFeature(c, out var feature) ? feature : MapFeature.StoneFloor;

    /// <summary>
    ///     Strict variant of <see cref="GlyphToFeature" /> for callers that need to
    ///     distinguish "this glyph has a concrete tile-feature identity" from "this glyph
    ///     resolves to plain floor by default". Returns <c>true</c> only for the terrain
    ///     glyphs (<c>.</c>, <c>,</c>, <c>#</c>, <c>A</c>, <c>~</c>) and the feature
    ///     glyphs in <see cref="FeatureGlyphs" /> that have a matching entry in
    ///     <see cref="RoomFeatureToMapFeature" />. Padding (<c>' '</c>), connection
    ///     identifiers, doorway candidates, wildcards, traps, and annotated glyphs return
    ///     <c>false</c> with <paramref name="feature" /> = <see cref="MapFeature.Default" />.
    /// </summary>
    private static bool TryGlyphToFeature(char c, out MapFeature feature)
    {
        switch (c)
        {
            case ' ':
                feature = MapFeature.Default;
                return true;
            case ',':
                feature = MapFeature.RockFloor;
                return true;
            case '.':
                feature = MapFeature.StoneFloor;
                return true;
            case '#':
                feature = MapFeature.StoneWall;
                return true;
            case 'A':
                feature = MapFeature.StoneArchway;
                return true;
            case '~':
                feature = MapFeature.Pool;
                return true;
        }

        if (FeatureGlyphs.TryGetValue(c, out var roomFeature)
            && roomFeature.HasValue
            && RoomFeatureToMapFeature.TryGetValue(roomFeature.Value, out feature))
        {
            return true;
        }

        feature = MapFeature.Default;
        return false;
    }

    /// <summary>
    ///     Shared linear strength for both demand and anti-tag matching. A demand match pulls a
    ///     fragment's weight up toward this factor and an anti-tag match pulls it down by the
    ///     reciprocal (<c>1 / DemandTagBonus</c>), so the two are exact opposites. Matches
    ///     accumulate linearly — a fragment matching <c>k</c> demands is favoured by
    ///     <c>1 + k * (DemandTagBonus - 1)</c>, so a single match yields this value and
    ///     additional matches add to it rather than compounding (<c>DemandTagBonus^k</c>).
    /// </summary>
    public const float DemandTagBonus = 5f;

    /// <summary>
    ///     Multiplier applied to a fragment carrying the <see cref="RoomFeatures.Trapped" />
    ///     tag (i.e. featuring one or more <c>^</c> trap glyphs) in a room that did not
    ///     ask for the <see cref="RoomFeatures.Trapped" /> feature. Traps are allowed in
    ///     any room (they overlay plain floor without contradicting other content), but
    ///     they're rarer than themed fragments by default so unsolicited trap fragments
    ///     get a sub-unit weight bias rather than the hard exclusion applied to other
    ///     undemanded placeable features.
    /// </summary>
    public const float UndemandedTrapPenalty = 0.2f;

    /// <summary>
    ///     Tile-count threshold (inclusive) below which a fragment is tagged
    ///     <c>size-small</c>. Counts payload tiles only (excludes conditional edge rows /
    ///     columns).
    /// </summary>
    public const int SizeSmallMaxTiles = 15;

    /// <summary>
    ///     Tile-count threshold (inclusive) below which a fragment is tagged
    ///     <c>size-medium</c>. Anything strictly larger is <c>size-large</c>.
    /// </summary>
    public const int SizeMediumMaxTiles = 49;

    /// <summary>
    ///     Per-tile weight added to a fragment's selection weight so that larger authored
    ///     fragments are picked more often, filling rooms with fewer, bigger chunks. The
    ///     fragment's static payload tile count (<see cref="PayloadArea" /> area) is
    ///     multiplied by this factor and added to the fragment's base DSL weight before the
    ///     edge / demand / variant multipliers. Dynamic fragments have an empty static payload (area 0), so
    ///     they receive no bonus.
    /// </summary>
    public const float SizeWeightBonus = 0.5f;

    private string? _generationWeight;

    /// <summary>
    ///     Synchronizes <see cref="EnsureInitialized" /> across concurrent first-touch
    ///     callers. Fragment instances returned by the production loaders are singletons
    ///     shared across tests / sessions; without this lock two threads racing the
    ///     <c>ByteMap == null</c> check would both run the parser and interleave their
    ///     writes to <see cref="Width" />, <see cref="Height" />, <see cref="PointToIndex" />,
    ///     <see cref="IndexToPoint" />, and <see cref="ByteMap" />, leaving an inconsistent
    ///     (size, lookup-table, payload) tuple.
    ///     <para>
    ///         <see cref="CreateVariant" /> uses <c>MemberwiseClone</c>, so variants
    ///         inherit the source's lock object. That's intentional and harmless: by the
    ///         time the clone exists the source is initialized, the variant's own
    ///         post-clone fields are reassigned by <see cref="ApplyTransformInPlace" />,
    ///         and <see cref="_initialized" /> is copied as <c>true</c> so the variant's
    ///         <see cref="EnsureInitialized" /> short-circuits.
    ///     </para>
    /// </summary>
    private readonly object _initLock = new();

    /// <summary>
    ///     Set to <c>true</c> as the very last write of a successful
    ///     <see cref="EnsureInitialized" /> call. Volatile so the fast-path lock-free
    ///     read has acquire semantics — once a thread observes <c>true</c>, it's
    ///     guaranteed to see every field that was written inside the lock.
    /// </summary>
    private volatile bool _initialized;

    public string Name
    {
        get;
        set;
    } = null!;

    /// <summary>
    ///     Name of the source fragment this instance was materialized from. Identical to
    ///     <see cref="Name" /> on source fragments; variants produced by
    ///     <see cref="CreateVariant" /> set this to the source's name so per-fragment
    ///     statistics (e.g. <see cref="Systems.Levels.GenerationStats.FragmentInstancesByName" />)
    ///     can aggregate placements across the whole orbit. Reads fall back to
    ///     <see cref="Name" /> when never assigned (e.g. on fragments that never went
    ///     through <see cref="EnsureInitialized" />).
    /// </summary>
    public string SourceName
    {
        get => field ?? Name;
        protected set;
    } = null!;

    public string Map
    {
        get;
        set;
    } = "";

    public DynamicMap? DynamicMap
    {
        get;
        set;
    }

    public byte[]? ByteMap
    {
        get;
        protected set;
    }

    public byte Width
    {
        get;
        protected set;
    }

    public byte Height
    {
        get;
        protected set;
    }

    public Rectangle PayloadArea
    {
        get;
        protected set;
    }

    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            ResetWeightFunction();
            _generationWeight = value;
        }
    }

    /// <summary>
    ///     Author-supplied descriptors plus auto-derived markers (size, edge categories) the
    ///     fragment carries. After <see cref="EnsureInitialized" />, this collection contains
    ///     only the positive tags — entries the author prefixed with <c>!</c> are stripped
    ///     out and moved into <see cref="AntiTags" />.
    /// </summary>
    public ICollection<string>? Tags
    {
        get;
        set;
    }

    /// <summary>
    ///     Opt-out tags parsed from author entries prefixed with <c>!</c> in
    ///     <see cref="Tags" />. Used by the placement gating pass to forbid this fragment in
    ///     a room whose <see cref="RoomFeatures" /> demands the matching non-placeable
    ///     feature (e.g. an entry <c>!cursed</c> blocks the fragment from
    ///     <see cref="RoomFeatures.Cursed" /> rooms). Shared across variants — the
    ///     transform doesn't affect it.
    /// </summary>
    public ICollection<string>? AntiTags
    {
        get;
        protected set;
    }

    /// <summary>
    ///     The subset of <see cref="Tags" /> that the auto-derivation pass
    ///     (<see cref="ApplyTagDerivations" />) produced from the payload - the size tag, the
    ///     content tags for feature/connection glyphs, and the per-orientation door tags. These
    ///     are not author-declared and are re-derived on every load, so they are excluded from the
    ///     authored serialization surface (see <see cref="ReduceTagsToAuthored" />). Has no public
    ///     setter, so it is never itself serialized.
    /// </summary>
    public IReadOnlyCollection<string>? DerivedTags
    {
        get;
        private set;
    }

    /// <summary>
    ///     Per-fragment annotation table: each key is a glyph used in <see cref="Map" /> and
    ///     the value is a non-empty string of substitute glyphs. At chunk placement time, for
    ///     every cell whose payload glyph matches a key, one substitute is picked uniformly at
    ///     random and the cell is realized as that substitute. Any glyph may be annotated —
    ///     including the standard terrain, feature, connection, and wildcard glyphs — so an
    ///     annotation can broaden or override a glyph's default meaning. Entries with an empty
    ///     substitution string are rejected at load.
    /// </summary>
    public Dictionary<char, string>? Annotations
    {
        get;
        set;
    }

    public bool NoRandomDoorways
    {
        get;
        set;
    }

    /// <summary>
    ///     Declares that this fragment is symmetric under reflection: every mirror image of
    ///     the payload is congruent to the original, so the variant expansion should skip
    ///     the four flip transforms and keep only the four pure rotations. See
    ///     <see cref="MapFragmentTransforms.GetOrbit" /> for the resolved orbit.
    /// </summary>
    public bool NoMirror
    {
        get;
        set;
    }

    /// <summary>
    ///     Declares that this fragment is symmetric under all four pure rotations (full
    ///     <see href="https://en.wikipedia.org/wiki/Cyclic_group">C4</see> rotational
    ///     symmetry): the variant expansion keeps the identity and a single mirror only.
    ///     Combined with <see cref="NoMirror" /> it collapses to a single variant.
    /// </summary>
    public bool NoTurnOver
    {
        get;
        set;
    }

    /// <summary>
    ///     Declares that this fragment should not be expanded into D4 variants at all — the
    ///     loader keeps only the identity. Implies <see cref="NoMirror" /> and
    ///     <see cref="NoTurnOver" />.
    /// </summary>
    public bool NoTransform
    {
        get;
        set;
    }

    /// <summary>
    ///     When <c>true</c> a <see cref="DenseRoomLayout" /> (or any layout that performs
    ///     wall-merge shifts between adjacent chunks) treats this fragment as an immovable
    ///     anchor: a neighboring chunk's candidate shift is rejected unconditionally if it
    ///     would move that neighbor on top of any cell of this fragment, regardless of
    ///     which cells would be overwritten. Authored statics that depend on exact wall
    ///     thickness (vaults, shrines, altar rooms with framed walls) opt in. Transform
    ///     variants inherit the flag from their source fragment via
    ///     <see cref="CreateVariant" />'s member-wise clone.
    /// </summary>
    public bool NoOverwrite
    {
        get;
        set;
    }

    /// <summary>
    ///     The D4 transform applied to materialize this fragment from its source. Defaults
    ///     to <see cref="MapFragmentTransform.Identity" /> for source fragments produced by
    ///     the loader. Variants set this to the transform that produced them.
    /// </summary>
    public MapFragmentTransform Transform
    {
        get;
        protected set;
    } = MapFragmentTransform.Identity;

    /// <summary>
    ///     Size of the symmetry orbit this fragment expands into when materialized by
    ///     <see cref="MapFragmentLoader{T}" />. A dynamic-map-only fragment has no authored geometry to
    ///     transform and therefore has one variant. Static maps derive their count from the transform-
    ///     exclusion flags via <see cref="MapFragmentTransforms.GetOrbit" />.
    /// </summary>
    public int VariantCount
        => !string.IsNullOrEmpty(Map)
            ? MapFragmentTransforms.GetOrbit(NoMirror, NoTurnOver, NoTransform).Length
            : 1;

    /// <summary>
    ///     Multiplier applied to the result of <c>GetWeight</c> so that the total pick
    ///     probability of an expanded orbit equals the source fragment's authored weight.
    ///     Computed as <c>1 / <see cref="VariantCount" /></c>.
    /// </summary>
    protected float VariantMultiplier => 1f / VariantCount;

    public bool ConditionalFirstRow
    {
        get;
        protected set;
    }

    public bool ConditionalLastRow
    {
        get;
        protected set;
    }

    public bool ConditionalFirstColumn
    {
        get;
        protected set;
    }

    public bool ConditionalLastColumn
    {
        get;
        protected set;
    }

    /// <summary>
    ///     Per-tile placement conditions parsed from the conditional first row, when set.
    ///     The array length matches <see cref="PayloadArea" />'s width.
    /// </summary>
    public EdgeConditionKind[]? NorthEdge
    {
        get;
        protected set;
    }

    /// <summary>
    ///     Per-tile placement conditions parsed from the conditional last row, when set.
    ///     The array length matches <see cref="PayloadArea" />'s width.
    /// </summary>
    public EdgeConditionKind[]? SouthEdge
    {
        get;
        protected set;
    }

    /// <summary>
    ///     Per-tile placement conditions parsed from the conditional first column, when set.
    ///     The array length matches <see cref="PayloadArea" />'s height.
    /// </summary>
    public EdgeConditionKind[]? WestEdge
    {
        get;
        protected set;
    }

    /// <summary>
    ///     Per-tile placement conditions parsed from the conditional last column, when set.
    ///     The array length matches <see cref="PayloadArea" />'s height.
    /// </summary>
    public EdgeConditionKind[]? EastEdge
    {
        get;
        protected set;
    }

    /// <summary>
    ///     Upper bound on the number of doorway tiles this fragment can supply to its
    ///     placed chunk. Computed at <see cref="EnsureInitialized" /> time.
    ///     <list type="bullet">
    ///         <item>
    ///             Dynamic fragments (no static <see cref="ByteMap" />) keep the default
    ///             <see cref="int.MaxValue" /> — <see cref="BuildChunk" /> enforces the
    ///             actual <c>minDoorways</c> at runtime.
    ///         </item>
    ///         <item>
    ///             Static fragments with one or more authored <c>?</c> markers store that
    ///             literal count — the placement-time random fallback never runs because
    ///             <see cref="TryPlaceStaticMap" /> only invokes it when the authored
    ///             count is zero.
    ///         </item>
    ///         <item>
    ///             Static fragments with no <c>?</c> markers and
    ///             <see cref="NoRandomDoorways" /> set to <c>false</c> store the number of
    ///             payload-perimeter wall tiles whose inward neighbor resolves to a floor
    ///             feature — a structural upper bound on what
    ///             <see cref="FindStaticMapDoorwayPoints" /> can ultimately pick. Cells
    ///             whose corridor-side neighbor ends up blocked at placement time are
    ///             still counted here; <see cref="TryPlaceStaticMap" /> rechecks.
    ///         </item>
    ///         <item>
    ///             Static fragments with no <c>?</c> markers and
    ///             <see cref="NoRandomDoorways" /> set to <c>true</c> store <c>0</c>.
    ///         </item>
    ///     </list>
    /// </summary>
    public int PossibleDoorwayCount
    {
        get;
        protected set;
    } = int.MaxValue;

    /// <summary>
    ///     <c>true</c> when at least one payload-perimeter cell is not a wall, doorway
    ///     candidate, or archway — i.e. the fragment's wall outline doesn't fully enclose
    ///     its inside cells. Such fragments dock against adjacent chunks (or the room
    ///     boundary's implicit wall) through their open perimeter cells without needing
    ///     an authored <c>+</c> doorway for intra-room connectivity. Inter-room
    ///     connection demands (<c>door-{n|s|e|w}</c> tags) still come from authored
    ///     <c>+</c> placements on the matching payload edge, so an open-perimeter
    ///     fragment that doesn't declare the demanded doorway direction simply loses the
    ///     direction bonus in <see cref="GetDemandMultiplier" /> rather than being
    ///     filtered out by <see cref="NormalMapFragment.GetWeight" />'s doorway gate.
    /// </summary>
    public bool HasOpenPerimeter
    {
        get;
        protected set;
    }

    public short[,]? PointToIndex
    {
        get;
        protected set;
    }

    public Point[]? IndexToPoint
    {
        get;
        protected set;
    }

    protected static readonly string DefaultWeight = "1";

    protected static readonly ParameterExpression BranchParameter =
        Expression.Parameter(typeof(string), name: "branch");

    protected static readonly ParameterExpression DepthParameter =
        Expression.Parameter(typeof(int), name: "depth");

    /// <summary>
    ///     Fragment's source name (i.e. <see cref="SourceName" />). Exposed to weight
    ///     expressions as <c>$sourceName</c> and used by the per-fragment translators
    ///     (<see cref="NormalMapFragment" />, <see cref="ConnectingMapFragment" />,
    ///     <see cref="DefiningMapFragment" />) to back the visitor-emitted
    ///     <c>$instances</c> / <c>$globalInstances</c> lookups against the per-name
    ///     instance dictionaries.
    /// </summary>
    protected static readonly ParameterExpression SourceNameParameter =
        Expression.Parameter(typeof(string), name: "sourceName");

    /// <summary>
    ///     Per-level instance counts keyed by fragment source name (mirrors
    ///     <see cref="GenerationStats.FragmentInstancesByName" />). Backs the
    ///     visitor-emitted <c>$instances</c> override on the per-fragment translators.
    /// </summary>
    protected static readonly ParameterExpression FragmentInstancesByNameParameter =
        Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), name: "fragmentInstancesByName");

    /// <summary>
    ///     Game-wide instance counts keyed by fragment source name (mirrors
    ///     <see cref="Game.FragmentInstancesByName" />). Backs the visitor-emitted
    ///     <c>$globalInstances</c> override on the per-fragment translators.
    /// </summary>
    protected static readonly ParameterExpression GlobalFragmentInstancesByNameParameter =
        Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), name: "globalFragmentInstancesByName");

    protected static readonly ParameterExpression TagInstancesParameter =
        Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), name: "tagInstances");

    /// <summary>
    ///     Game-wide instance counts keyed by fragment tag (mirrors
    ///     <see cref="Game.FragmentInstancesByTag" />). Exposed to weight expressions as
    ///     <c>$globalTagInstances[...]</c>; the indexer falls back to <c>0</c> for
    ///     unseen keys via <see cref="System.Collections.Generic.CollectionExtensions.GetValueOrDefault{TKey,TValue}(System.Collections.Generic.IReadOnlyDictionary{TKey,TValue},TKey)" />.
    /// </summary>
    protected static readonly ParameterExpression GlobalTagInstancesParameter =
        Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), name: "globalTagInstances");

    /// <summary>
    ///     Game-wide instance counts keyed by <see cref="RoomFeatures" /> bit (mirrors
    ///     <see cref="GenerationStats.FragmentInstancesByFeature" />). Exposed to weight
    ///     expressions as <c>$globalFeatureInstances[...]</c>; lets authors damp specific
    ///     themed features once they've already appeared a few times in the run (e.g. to
    ///     prevent multiple deity-altar rooms on a single playthrough). The indexer falls
    ///     back to <c>0</c> for unseen keys.
    /// </summary>
    protected static readonly ParameterExpression GlobalFeatureInstancesParameter =
        Expression.Parameter(typeof(IReadOnlyDictionary<RoomFeatures, int>), name: "globalFeatureInstances");

    protected static readonly ParameterExpression ConnectionParameter =
        Expression.Parameter(typeof(ConnectionComponent), name: "connection");

    /// <summary>
    ///     <see cref="RoomFeatures" /> bitmask of the room being filled. Used by per-room
    ///     fragment weight expressions in <see cref="DefiningMapFragment" />.
    /// </summary>
    protected static readonly ParameterExpression RoomFeaturesParameter =
        Expression.Parameter(typeof(RoomFeatures), name: "roomFeatures");

    private static readonly MethodInfo GetStringIntOrDefaultMethod = typeof(MapFragment)
        .GetMethod(nameof(GetStringIntOrDefault), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static int GetStringIntOrDefault(IReadOnlyDictionary<string, int> values, string key)
        => values.GetValueOrDefault(key);

    /// <summary>
    ///     Builds an <see cref="UnicornExpressionVisitor" /> that exposes the shared
    ///     <c>$instances</c> / <c>$globalInstances</c> overrides on top of the supplied
    ///     parameter list. The overrides bake a <c>GetValueOrDefault(dict, sourceName)</c>
    ///     call into the compiled IL only when the expression actually references the
    ///     variable — fragments whose DSL never mentions <c>$instances</c> pay nothing.
    /// </summary>
    protected static UnicornExpressionVisitor CreateInstanceAwareTranslator(params ParameterExpression[] parameters)
        => new(
            parameters,
            new Dictionary<string, Expression>(StringComparer.Ordinal)
            {
                ["instances"] = Expression.Call(
                    null, GetStringIntOrDefaultMethod,
                    FragmentInstancesByNameParameter, SourceNameParameter),
                ["globalInstances"] = Expression.Call(
                    null, GetStringIntOrDefaultMethod,
                    GlobalFragmentInstancesByNameParameter, SourceNameParameter)
            });

    /// <summary>
    ///     Compiles <see cref="GenerationWeight" /> (or <see cref="DefaultWeight" /> when
    ///     unset) into <typeparamref name="TDelegate" /> via the supplied translator, wrapping
    ///     any parse-time exception with this fragment's name. Called at most once per
    ///     fragment lifetime — subclasses memoize the result in their own typed field.
    /// </summary>
    protected TDelegate CompileWeightFunction<TDelegate>(UnicornExpressionVisitor translator)
        where TDelegate : Delegate
    {
        try
        {
            return translator.Translate<TDelegate, float>(GenerationWeight ?? DefaultWeight);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
        }
    }

    protected abstract void ResetWeightFunction();

    /// <summary>
    ///     Forces compilation of this fragment's <see cref="GenerationWeight" /> delegate when
    ///     it hasn't been compiled yet. <see cref="MapFragmentLoader{T}" /> calls this on each
    ///     source fragment BEFORE expanding its D4 orbit, so the compiled delegate is shared by
    ///     every variant through <see cref="CreateVariant" />'s member-wise clone .
    /// </summary>
    public virtual void EnsureWeightFunctionCompiled()
    {
    }

    /// <summary>
    ///     Parses <see cref="Map" /> into <see cref="ByteMap" /> on first call and lazily
    ///     populates <see cref="PointToIndex" /> / <see cref="IndexToPoint" />. The lookup
    ///     tables are memoized by <c>(Width, Height)</c> in a process-wide cache shared
    ///     across all <see cref="Game" /> instances.
    ///     <para>
    ///         Thread-safe via double-checked locking on <see cref="_initLock" />: the
    ///         volatile <see cref="_initialized" /> flag short-circuits the fast path once
    ///         any thread has finished initialization. Production fragments are shared
    ///         singletons across tests / game sessions, so concurrent first-touch is
    ///         possible (and was the source of intermittent test failures from interleaved
    ///         writes to <see cref="Width" />, <see cref="Height" />,
    ///         <see cref="PointToIndex" />, and <see cref="ByteMap" />).
    ///     </para>
    /// </summary>
    [MemberNotNull(nameof(ByteMap))]
    [MemberNotNull(nameof(PointToIndex))]
    [MemberNotNull(nameof(IndexToPoint))]
    public void EnsureInitialized()
    {
        if (_initialized)
        {
            Debug.Assert(ByteMap is not null);
            Debug.Assert(PointToIndex is not null);
            Debug.Assert(IndexToPoint is not null);
            return;
        }

        lock (_initLock)
        {
            if (_initialized)
            {
                Debug.Assert(ByteMap is not null);
                Debug.Assert(PointToIndex is not null);
                Debug.Assert(IndexToPoint is not null);
                return;
            }

            InitializeCore();
            // Final write under the lock; the volatile semantics publish every preceding
            // field assignment to readers that observe _initialized == true.
            _initialized = true;
        }
    }

    [MemberNotNull(nameof(ByteMap))]
    [MemberNotNull(nameof(PointToIndex))]
    [MemberNotNull(nameof(IndexToPoint))]
    private void InitializeCore()
    {
        if (ByteMap == null)
        {
            byte x = 0, y = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Map.Length; i++)
            {
                switch (Map[i])
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        if (x > Width)
                        {
                            Width = x;
                        }

                        x = 0;
                        y++;
                        continue;
                    default:
                        x++;
                        break;
                }
            }

            if (x > 0)
            {
                if (x > Width)
                {
                    Width = x;
                }

                y++;
            }

            if (y > Height)
            {
                Height = y;
            }

            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(Width, Height);

            x = 0;
            y = 0;
            ByteMap = new byte[Width * Height];
            // Track which of the four map corners carries a '~' marker. An edge is
            // conditional only when BOTH of its corners are '~' — a single corner is
            // ambiguous (could equally indicate either of the two adjacent edges).
            var tildeTopLeft = false;
            var tildeTopRight = false;
            var tildeBottomLeft = false;
            var tildeBottomRight = false;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Map.Length; i++)
            {
                var character = Map[i];
                switch (character)
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        while (x != Width)
                        {
                            ByteMap[PointToIndex[x, y]] = (byte)' ';
                            x++;
                        }

                        x = 0;
                        y++;
                        continue;
                    case '~':
                        // '~' at a map corner cooperates with the other corner of an
                        // adjacent edge to mark that edge as conditional. An edge is
                        // conditional only when BOTH of its corners are '~' (resolved
                        // after the parse loop). Anywhere else, '~' is an authored
                        // Pool tile (handled later by Write).
                        var isLeftEdge = x == 0;
                        var isRightEdge = x == Width - 1;
                        var isTopEdge = y == 0;
                        var isBottomEdge = y == Height - 1;
                        var isCorner = (isLeftEdge || isRightEdge) && (isTopEdge || isBottomEdge);
                        if (isCorner)
                        {
                            tildeTopLeft |= isLeftEdge && isTopEdge;
                            tildeTopRight |= isRightEdge && isTopEdge;
                            tildeBottomLeft |= isLeftEdge && isBottomEdge;
                            tildeBottomRight |= isRightEdge && isBottomEdge;
                        }

                        ByteMap[PointToIndex[x, y]] = (byte)character;
                        x++;
                        break;
                    default:
                        ByteMap[PointToIndex[x, y]] = (byte)character;
                        x++;
                        break;
                }
            }

            if (Width == 0)
            {
                return;
            }

            if (x > 0)
            {
                while (x != Width)
                {
                    ByteMap[x + Width * y] = (byte)' ';
                    x++;
                }
            }

            // Resolve which edges are conditional: an edge counts only when BOTH of its
            // corners carry '~'. The corner cells themselves are NOT edge slots — the
            // parsed edge runs across the non-corner cells of the conditional row /
            // column. The corner is purely a marker.
            ConditionalFirstRow = tildeTopLeft && tildeTopRight;
            ConditionalLastRow = tildeBottomLeft && tildeBottomRight;
            ConditionalFirstColumn = tildeTopLeft && tildeBottomLeft;
            ConditionalLastColumn = tildeTopRight && tildeBottomRight;

            var payloadOrigin = new Point((byte)(ConditionalFirstColumn ? 1 : 0),
                (byte)(ConditionalFirstRow ? 1 : 0));
            PayloadArea = new Rectangle(payloadOrigin,
                (byte)(Width - payloadOrigin.X - (ConditionalLastColumn ? 1 : 0)),
                (byte)(Height - payloadOrigin.Y - (ConditionalLastRow ? 1 : 0)));

            ParseEdgeConditions();

            // ApplyTagDerivations scans the payload to emit content/door tags; it also
            // returns the count of '+' glyphs inside the payload.
            var payloadDoorwayCount = ApplyTagDerivations();
            PossibleDoorwayCount = payloadDoorwayCount > 0
                ? payloadDoorwayCount
                : NoRandomDoorways
                    ? 0
                    : CountPossibleRandomDoorways();

            HasOpenPerimeter = ComputeHasOpenPerimeter();

            ValidateAnnotations();
        }

        if (PointToIndex == null
            || IndexToPoint == null)
        {
            (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(Width, Height);
        }
    }

    /// <summary>
    ///     Validates the optional <see cref="Annotations" /> table after the byte map has
    ///     been parsed. Rejects entries whose substitution string is empty.
    /// </summary>
    private void ValidateAnnotations()
    {
        if (Annotations is null)
        {
            return;
        }

        foreach (var (glyph, substitutes) in Annotations)
        {
            if (string.IsNullOrEmpty(substitutes))
            {
                throw new InvalidOperationException(
                    $"{Name}: annotation '{glyph}' has an empty substitution string.");
            }
        }
    }

    /// <summary>
    ///     Asserts that a static fragment intended for surrounding-chunk placement (i.e.
    ///     anything other than a <see cref="DefiningMapFragment" />) can actually supply
    ///     doorways or dock against neighbors. A static fragment with <c>0</c>
    ///     <see cref="PossibleDoorwayCount" /> and no <see cref="HasOpenPerimeter" /> can
    ///     never satisfy a <see cref="LotPlacementContext.MinDoorways" /> &gt; 0 lot and
    ///     never connects to a neighbor through an open edge - the weight gate in
    ///     <c>NormalMapFragment.GetWeight</c> / <c>ConnectingMapFragment.GetWeight</c>
    ///     silently returns <c>0</c>, so the fragment would be loaded but never picked.
    ///     Failing at load makes the authoring bug visible.
    ///     <para>
    ///         Dynamic fragments are exempt (<see cref="PossibleDoorwayCount" /> is
    ///         <see cref="int.MaxValue" /> so the check never trips). Defining fragments
    ///         are exempt because they are placed as the room's only / first chunk and
    ///         doorway demand is satisfied by surrounding fragments.
    ///     </para>
    ///     <para>
    ///         Invoked by <see cref="MapFragmentLoader{T}.Load" /> after initialization so
    ///         only fragments materialised from the production data tree are checked;
    ///         test fixtures that construct synthetic fragments are not subject to the
    ///         placement-capability contract.
    ///     </para>
    /// </summary>
    public void ValidatePlacementCapability()
    {
        if (this is DefiningMapFragment
            || ByteMap is null
            || ByteMap.Length == 0)
        {
            return;
        }

        if (!HasOpenPerimeter && PossibleDoorwayCount == 0)
        {
            throw new InvalidOperationException(
                $"'{Name}': fragment has no authored doorways ('+'), "
                + "NoRandomDoorways=true (or no wall-floor pair on its perimeter), "
                + "and no open perimeter. It can never connect to a neighbor or satisfy "
                + "a doorway demand, so layouts will never pick it. Author at least one "
                + "'+', clear NoRandomDoorways, or open one perimeter edge.");
        }
    }

    /// <summary>
    ///     Runs once on first <see cref="EnsureInitialized" /> call. Splits any author
    ///     <see cref="Tags" /> entries prefixed with <c>!</c> into <see cref="AntiTags" />,
    ///     normalises positive tags to case-insensitive, and adds the auto-derived tag set:
    ///     <c>size-*</c> from the payload's tile count and content tags (<c>altar</c>,
    ///     <c>pedestal</c>, <c>stand</c>, <c>entrance</c>, <c>exit</c>) derived
    ///     from feature & connection glyphs in the payload. Auto tags that depend on the
    ///     variant's orientation (door directions, axis-named symmetry) remain deferred.
    /// </summary>
    private int ApplyTagDerivations()
    {
        if (ByteMap is null || Width == 0 || Height == 0)
        {
            return 0;
        }

        var positives = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        HashSet<string>? negatives = null;
        if (Tags is not null)
        {
            foreach (var raw in Tags)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var trimmed = raw.Trim();
                if (trimmed[0] == '!')
                {
                    var negative = trimmed[1..].Trim();
                    if (negative.Length == 0)
                    {
                        throw new InvalidOperationException($"{Name}: empty '!' tag entry.");
                    }

                    (negatives ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(negative);
                }
                else
                {
                    positives.Add(trimmed);
                }
            }
        }

        var payloadTileCount = PayloadArea.Width * PayloadArea.Height;
        var derived = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sizeTag = payloadTileCount <= SizeSmallMaxTiles
            ? "size-small"
            : payloadTileCount <= SizeMediumMaxTiles
                ? "size-medium"
                : "size-large";
        positives.Add(sizeTag);
        derived.Add(sizeTag);

        // Content auto-tags derived from feature & connection glyphs in the authored payload.
        // Authors can still negate them via the `!`-prefix mechanism above. Content tags are
        // transform-invariant: the same glyphs appear in every variant of an orbit. Each
        // qualifying glyph contributes both a generic family tag (e.g. `fountain`) and the
        // specific feature tag (e.g. `small-fountain`) so authors can match either. Doorway
        // glyphs sitting on a payload edge additionally emit a per-direction
        // <c>door-n</c> / <c>door-s</c> / <c>door-e</c> / <c>door-w</c> tag for the current
        // orientation; <see cref="CreateVariant" /> reruns this method to re-derive these
        // for rotated/mirrored variants.
        var area = PayloadArea;
        var doorNorth = false;
        var doorSouth = false;
        var doorWest = false;
        var doorEast = false;
        var payloadDoorwayCount = 0;
        for (var y = area.TopLeft.Y; y <= area.BottomRight.Y; y++)
        {
            for (var x = area.TopLeft.X; x <= area.BottomRight.X; x++)
            {
                var glyph = (char)ByteMap![PointToIndex![x, y]];
                // Generic family tag (e.g. `fountain`, `stand`, `pedestal`, `shrine`,
                // `altar`, `entrance`, `exit`, `trap`); the feature wildcard
                // glyph contributes the wildcard demand tag.
                var generic = glyph switch
                {
                    '_' => "altar",
                    '\\' or '/' or '&' => "shrine",
                    '{' or '}' => "fountain",
                    '?' or '!' or '"' or '=' => "pedestal",
                    '[' or ']' or '(' or ':' or ')' or ';' => "stand",
                    '<' => "entrance",
                    '>' when !positives.Contains("shortcut") => "exit",
                    '^' => "trap",
                    FeatureWildcardGlyph => WildcardFeatureTag,
                    _ => null
                };

                if (generic is not null)
                {
                    positives.Add(generic);
                    derived.Add(generic);
                }

                // Specific feature tag (e.g. `small-fountain`, `weapon-stand`).
                if (FeatureGlyphs.TryGetValue(glyph, out var feature)
                    && feature.HasValue
                    && FeatureTagNames.TryGetValue(feature.Value, out var canonical))
                {
                    positives.Add(canonical);
                    derived.Add(canonical);
                }

                if (glyph == DoorwayGlyph)
                {
                    payloadDoorwayCount++;
                    if (y == area.TopLeft.Y)
                    {
                        doorNorth = true;
                    }
                    if (y == area.BottomRight.Y)
                    {
                        doorSouth = true;
                    }
                    if (x == area.TopLeft.X)
                    {
                        doorWest = true;
                    }
                    if (x == area.BottomRight.X)
                    {
                        doorEast = true;
                    }
                }
            }
        }

        if (doorNorth)
        {
            positives.Add("door-n");
            derived.Add("door-n");
        }
        if (doorSouth)
        {
            positives.Add("door-s");
            derived.Add("door-s");
        }
        if (doorWest)
        {
            positives.Add("door-w");
            derived.Add("door-w");
        }
        if (doorEast)
        {
            positives.Add("door-e");
            derived.Add("door-e");
        }

        Tags = positives;
        AntiTags = negatives;
        DerivedTags = derived;
        return payloadDoorwayCount;
    }

    /// <summary>
    ///     Reduces <see cref="Tags" /> to the author-declared surface for serialization: drops every
    ///     auto-<see cref="DerivedTags" /> entry (the size / content / door markers that
    ///     <see cref="ApplyTagDerivations" /> re-produces from the payload on load) and re-encodes each
    ///     <see cref="AntiTags" /> opt-out as its authored <c>!</c>-prefixed entry. Clears the collection
    ///     to <see langword="null" /> when nothing author-declared remains. Called by the editor before
    ///     serializing a source fragment so generated markers never leak back into the data files.
    /// </summary>
    public void ReduceTagsToAuthored()
    {
        var authored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (Tags is not null)
        {
            foreach (var tag in Tags)
            {
                if (DerivedTags is null || !DerivedTags.Contains(tag))
                {
                    authored.Add(tag);
                }
            }
        }

        if (AntiTags is not null)
        {
            foreach (var antiTag in AntiTags)
            {
                authored.Add("!" + antiTag);
            }
        }

        Tags = authored.Count == 0 ? null : authored;
    }

    /// <summary>
    ///     Materializes a D4 variant of this fragment by applying <paramref name="transform" />
    ///     to its initialized ByteMap, payload area, conditional-edge flags, and per-tile
    ///     edge condition arrays. The returned fragment is a shallow clone with its own
    ///     transformed geometry; immutable metadata (weight expression, tags, connections)
    ///     is shared. The source is auto-initialized if needed.
    ///     <para>
    ///         Returns the source instance unchanged when <paramref name="transform" /> is
    ///         <see cref="MapFragmentTransform.Identity" />.
    ///     </para>
    /// </summary>
    public MapFragment CreateVariant(MapFragmentTransform transform)
    {
        EnsureInitialized();

        if (transform == MapFragmentTransform.Identity)
        {
            return this;
        }

        var variant = (MapFragment)MemberwiseClone();
        variant.ApplyTransformInPlace(transform);
        variant.Transform = transform;
        variant.Name = $"{Name}_{transform}";
        variant.SourceName = SourceName;
        // Strip the source's orientation-dependent door-* tags before re-deriving them
        // from the rotated payload. AntiTags are intentionally shared by reference —
        // opt-out tags are author declarations on the fragment family, not per-orientation
        // state. Content/size/symmetry tags are transform-invariant; ApplyTagDerivations
        // re-emits them identically from the transformed payload.
        if (Tags is not null)
        {
            variant.Tags = new HashSet<string>(
                Tags.Where(t => !t.StartsWith("door-", StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            variant.Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        variant.ApplyTagDerivations();
        return variant;
    }

    private void ApplyTransformInPlace(MapFragmentTransform transform)
    {
        var sourceWidth = Width;
        var sourceHeight = Height;
        var sourceByteMap = ByteMap!;
        var sourcePointToIndex = PointToIndex!;

        var (newWidth, newHeight) = transform.GetTransformedSize(sourceWidth, sourceHeight);
        Width = newWidth;
        Height = newHeight;

        (PointToIndex, IndexToPoint) = Rectangle.GetPointIndex(newWidth, newHeight);

        var newByteMap = new byte[newWidth * newHeight];
        for (byte y = 0; y < sourceHeight; y++)
        {
            for (byte x = 0; x < sourceWidth; x++)
            {
                var destination = transform.Apply(new Point(x, y), sourceWidth, sourceHeight);
                newByteMap[PointToIndex[destination.X, destination.Y]] =
                    sourceByteMap[sourcePointToIndex[x, y]];
            }
        }

        ByteMap = newByteMap;

        var transformedTopLeft = transform.Apply(PayloadArea.TopLeft, sourceWidth, sourceHeight);
        var transformedBottomRight = transform.Apply(PayloadArea.BottomRight, sourceWidth, sourceHeight);
        PayloadArea = new Rectangle(
            new Point(Math.Min(transformedTopLeft.X, transformedBottomRight.X),
                Math.Min(transformedTopLeft.Y, transformedBottomRight.Y)),
            new Point(Math.Max(transformedTopLeft.X, transformedBottomRight.X),
                Math.Max(transformedTopLeft.Y, transformedBottomRight.Y)));

        TransformEdges(transform);
    }

    private void TransformEdges(MapFragmentTransform transform)
    {
        var sourceNorth = NorthEdge;
        var sourceSouth = SouthEdge;
        var sourceWest = WestEdge;
        var sourceEast = EastEdge;
        var sourceConditionalFirstRow = ConditionalFirstRow;
        var sourceConditionalLastRow = ConditionalLastRow;
        var sourceConditionalFirstColumn = ConditionalFirstColumn;
        var sourceConditionalLastColumn = ConditionalLastColumn;

        NorthEdge = null;
        SouthEdge = null;
        WestEdge = null;
        EastEdge = null;
        ConditionalFirstRow = false;
        ConditionalLastRow = false;
        ConditionalFirstColumn = false;
        ConditionalLastColumn = false;

        AssignTransformedEdge(transform, FragmentEdge.North, sourceNorth, sourceConditionalFirstRow);
        AssignTransformedEdge(transform, FragmentEdge.South, sourceSouth, sourceConditionalLastRow);
        AssignTransformedEdge(transform, FragmentEdge.West, sourceWest, sourceConditionalFirstColumn);
        AssignTransformedEdge(transform, FragmentEdge.East, sourceEast, sourceConditionalLastColumn);
    }

    private void AssignTransformedEdge(MapFragmentTransform transform, FragmentEdge source,
        EdgeConditionKind[]? edge, bool conditional)
    {
        var (destination, reverse) = MapEdgeUnderTransform(transform, source);
        var transformed = edge is null
            ? null
            : reverse ? ReverseCopy(edge) : (EdgeConditionKind[])edge.Clone();

        switch (destination)
        {
            case FragmentEdge.North:
                NorthEdge = transformed;
                ConditionalFirstRow = conditional;
                break;
            case FragmentEdge.South:
                SouthEdge = transformed;
                ConditionalLastRow = conditional;
                break;
            case FragmentEdge.West:
                WestEdge = transformed;
                ConditionalFirstColumn = conditional;
                break;
            case FragmentEdge.East:
                EastEdge = transformed;
                ConditionalLastColumn = conditional;
                break;
        }
    }

    private static EdgeConditionKind[] ReverseCopy(EdgeConditionKind[] source)
    {
        var result = new EdgeConditionKind[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            result[i] = source[source.Length - 1 - i];
        }

        return result;
    }

    /// <summary>
    ///     Returns where a source edge (N/S/W/E) lands and whether its per-tile array must
    ///     be reversed after applying <paramref name="transform" />. Derived geometrically
    ///     from <see cref="MapFragmentTransforms.Apply" /> by tracking each source edge's
    ///     first and last cell through the point transform.
    /// </summary>
    private static (FragmentEdge Destination, bool Reverse) MapEdgeUnderTransform(
        MapFragmentTransform transform, FragmentEdge source)
        => (transform, source) switch
        {
            (MapFragmentTransform.Identity, _) => (source, false),

            (MapFragmentTransform.Rotate90, FragmentEdge.North) => (FragmentEdge.East, false),
            (MapFragmentTransform.Rotate90, FragmentEdge.East) => (FragmentEdge.South, true),
            (MapFragmentTransform.Rotate90, FragmentEdge.South) => (FragmentEdge.West, false),
            (MapFragmentTransform.Rotate90, FragmentEdge.West) => (FragmentEdge.North, true),

            (MapFragmentTransform.Rotate180, FragmentEdge.North) => (FragmentEdge.South, true),
            (MapFragmentTransform.Rotate180, FragmentEdge.East) => (FragmentEdge.West, true),
            (MapFragmentTransform.Rotate180, FragmentEdge.South) => (FragmentEdge.North, true),
            (MapFragmentTransform.Rotate180, FragmentEdge.West) => (FragmentEdge.East, true),

            (MapFragmentTransform.Rotate270, FragmentEdge.North) => (FragmentEdge.West, true),
            (MapFragmentTransform.Rotate270, FragmentEdge.East) => (FragmentEdge.North, false),
            (MapFragmentTransform.Rotate270, FragmentEdge.South) => (FragmentEdge.East, true),
            (MapFragmentTransform.Rotate270, FragmentEdge.West) => (FragmentEdge.South, false),

            (MapFragmentTransform.FlipHorizontal, FragmentEdge.North) => (FragmentEdge.North, true),
            (MapFragmentTransform.FlipHorizontal, FragmentEdge.East) => (FragmentEdge.West, false),
            (MapFragmentTransform.FlipHorizontal, FragmentEdge.South) => (FragmentEdge.South, true),
            (MapFragmentTransform.FlipHorizontal, FragmentEdge.West) => (FragmentEdge.East, false),

            (MapFragmentTransform.FlipVertical, FragmentEdge.North) => (FragmentEdge.South, false),
            (MapFragmentTransform.FlipVertical, FragmentEdge.East) => (FragmentEdge.East, true),
            (MapFragmentTransform.FlipVertical, FragmentEdge.South) => (FragmentEdge.North, false),
            (MapFragmentTransform.FlipVertical, FragmentEdge.West) => (FragmentEdge.West, true),

            (MapFragmentTransform.FlipDiagonal, FragmentEdge.North) => (FragmentEdge.West, false),
            (MapFragmentTransform.FlipDiagonal, FragmentEdge.East) => (FragmentEdge.South, false),
            (MapFragmentTransform.FlipDiagonal, FragmentEdge.South) => (FragmentEdge.East, false),
            (MapFragmentTransform.FlipDiagonal, FragmentEdge.West) => (FragmentEdge.North, false),

            (MapFragmentTransform.FlipAntiDiagonal, FragmentEdge.North) => (FragmentEdge.East, true),
            (MapFragmentTransform.FlipAntiDiagonal, FragmentEdge.East) => (FragmentEdge.North, true),
            (MapFragmentTransform.FlipAntiDiagonal, FragmentEdge.South) => (FragmentEdge.West, true),
            (MapFragmentTransform.FlipAntiDiagonal, FragmentEdge.West) => (FragmentEdge.South, true),

            _ => throw new ArgumentOutOfRangeException(nameof(transform), transform, null)
        };

    private enum FragmentEdge
    {
        North,
        South,
        West,
        East
    }

    /// <summary>
    ///     Returns the parsed edge condition array for the requested cardinal direction, or
    ///     <c>null</c> when that edge is not marked conditional (both corner cells must be
    ///     <c>~</c>). Only the four cardinal <see cref="Direction" /> values are accepted;
    ///     diagonals, <see cref="Direction.Up" />, and <see cref="Direction.Down" /> throw.
    /// </summary>
    public EdgeConditionKind[]? GetEdge(Direction direction)
        => direction switch
        {
            Direction.North => NorthEdge,
            Direction.South => SouthEdge,
            Direction.East => EastEdge,
            Direction.West => WestEdge,
            _ => throw new ArgumentOutOfRangeException(nameof(direction),
                $"Only cardinal directions are supported for edge lookup; got {direction}.")
        };

    private void ParseEdgeConditions()
    {
        if (ConditionalFirstRow)
        {
            NorthEdge = ParseHorizontalEdge(rowY: 0);
        }

        if (ConditionalLastRow)
        {
            SouthEdge = ParseHorizontalEdge(rowY: (byte)(Height - 1));
        }

        if (ConditionalFirstColumn)
        {
            WestEdge = ParseVerticalEdge(colX: 0);
        }

        if (ConditionalLastColumn)
        {
            EastEdge = ParseVerticalEdge(colX: (byte)(Width - 1));
        }
    }

    private EdgeConditionKind[] ParseHorizontalEdge(byte rowY)
    {
        var width = PayloadArea.Width;
        var result = new EdgeConditionKind[width];
        var startX = PayloadArea.TopLeft.X;
        // The payload tile adjacent to the conditional row is one row inward
        // (rowY + 1 for the first row, rowY - 1 for the last row). Used to resolve
        // 'S' to the equivalent tile-specific condition at parse time.
        var adjacentY = rowY == 0 ? rowY + 1 : rowY - 1;
        for (var i = 0; i < width; i++)
        {
            var c = (char)ByteMap![PointToIndex![startX + i, rowY]];
            result[i] = c == 'S'
                ? ResolveSameAtPayloadTile(startX + i, adjacentY)
                : TranslateEdgeChar(c);
        }

        return result;
    }

    private EdgeConditionKind[] ParseVerticalEdge(byte colX)
    {
        var height = PayloadArea.Height;
        var result = new EdgeConditionKind[height];
        var startY = PayloadArea.TopLeft.Y;
        // The payload tile adjacent to the conditional column is one column inward
        // (colX + 1 for the first column, colX - 1 for the last column). Used to
        // resolve 'S' to the equivalent tile-specific condition at parse time.
        var adjacentX = colX == 0 ? colX + 1 : colX - 1;
        for (var i = 0; i < height; i++)
        {
            var c = (char)ByteMap![PointToIndex![colX, startY + i]];
            result[i] = c == 'S'
                ? ResolveSameAtPayloadTile(adjacentX, startY + i)
                : TranslateEdgeChar(c);
        }

        return result;
    }

    /// <summary>
    ///     Translates an <c>S</c> edge symbol to the tile-specific
    ///     <see cref="EdgeConditionKind" /> that matches the payload feature at the
    ///     given coordinates. <c>S</c> is the only authoring symbol whose semantic
    ///     ("neighbor must equal this fragment's payload edge tile feature") is
    ///     context-dependent, so it's resolved at parse time against the payload tile
    ///     immediately inward from the conditional row/column. The result has the
    ///     same numeric value as the matching <see cref="MapFeature" />.
    ///     <para>
    ///         Throws when the adjacent payload glyph has no concrete tile feature to
    ///         mirror: connection / doorway-candidate / wildcard / trap / creature /
    ///         padding glyphs all sit on plain <see cref="MapFeature.StoneFloor" /> by
    ///         convention rather than carrying a feature identity in their own right,
    ///         and an author who wrote an <c>S</c> next to one almost certainly meant
    ///         to author the explicit condition glyph (<c>.</c>) instead.
    ///     </para>
    /// </summary>
    private EdgeConditionKind ResolveSameAtPayloadTile(int x, int y)
    {
        var glyph = (char)ByteMap![PointToIndex![x, y]];
        if (!TryGlyphToFeature(glyph, out var feature))
        {
            throw new InvalidOperationException(
                $"{Name}: 'S' edge condition cannot resolve adjacent payload glyph '{glyph}' "
                + "to a specific MapFeature. Author the explicit condition glyph instead.");
        }
        return (EdgeConditionKind)feature;
    }

    /// <summary>
    ///     Mirrors <see cref="FindStaticMapDoorwayPoints" />'s structural test against the
    ///     payload-only view of the static map: counts payload-perimeter <c>#</c> tiles whose
    ///     inward orthogonal neighbor resolves to a floor feature (StoneFloor or RockFloor)
    ///     and skips the four payload corners. Outside-cell corridor reachability isn't known
    ///     at load time, so this is an upper bound — <see cref="TryPlaceStaticMap" /> performs
    ///     the runtime check.
    /// </summary>
    private int CountPossibleRandomDoorways()
    {
        var count = 0;
        foreach (var (edge, inside, _) in EnumeratePerimeterEdgeCells(PayloadArea))
        {
            if ((char)ByteMap![PointToIndex![edge.X, edge.Y]] != '#')
            {
                continue;
            }

            var insideFeature = GlyphToFeature((char)ByteMap[PointToIndex[inside.X, inside.Y]]);
            if (insideFeature == MapFeature.StoneFloor || insideFeature == MapFeature.RockFloor)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    ///     Returns <c>true</c> when at least one payload-perimeter cell (corners included)
    ///     is not a wall (<c>#</c>), authored doorway (<c>+</c>), or archway (<c>A</c>) —
    ///     i.e. the fragment relies on adjacent chunks (or the room boundary) to close
    ///     its perimeter. Drives <see cref="HasOpenPerimeter" />.
    /// </summary>
    private bool ComputeHasOpenPerimeter()
    {
        if (ByteMap is null || ByteMap.Length == 0)
        {
            // Dynamic fragments draw their own perimeter at placement time; treat as
            // fully enclosed for filter purposes.
            return false;
        }

        var area = PayloadArea;
        if (area.Width == 0 || area.Height == 0)
        {
            return false;
        }

        var topLeft = area.TopLeft;
        var bottomRight = area.BottomRight;
        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                if (x != topLeft.X && x != bottomRight.X && y != topLeft.Y && y != bottomRight.Y)
                {
                    // Skip interior cells; only payload-perimeter cells participate.
                    continue;
                }

                var glyph = (char)ByteMap[PointToIndex![x, y]];
                if (glyph != '#' && glyph != '+' && glyph != 'A' && glyph != ' ')
                {
                    return true;
                }
            }
        }

        return false;
    }

    private EdgeConditionKind TranslateEdgeChar(char c)
        => c switch
        {
            '~' or ' ' => EdgeConditionKind.Default,
            'F' => EdgeConditionKind.Filled,
            'U' => EdgeConditionKind.Unfilled,
            'E' => EdgeConditionKind.Edge,
            '#' => EdgeConditionKind.StoneWall,
            ',' => EdgeConditionKind.RockFloor,
            '.' => EdgeConditionKind.StoneFloor,
            '+' => EdgeConditionKind.Doorway,
            'A' => EdgeConditionKind.StoneArchway,
            // 'S' is handled before reaching this point, so it never appears here.
            _ => throw new InvalidOperationException(
                $"{Name}: invalid edge condition symbol '{c}'. Allowed: '~', '.', '+', ' ', 'S', 'F', 'U', 'E', '#', ',', 'A'.")
        };

    /// <summary>
    ///     Tie-breaker bonus per successfully matched edge slot — see <see cref="GetEdgeMatchMultiplier" />.
    /// </summary>
    public const float EdgeMatchTieBreakerAlpha = 0.1f;

    /// <summary>
    ///     Returns a non-negative weight multiplier for placing this fragment in the lot
    ///     described by <paramref name="ctx" />. <c>0</c> means no anchor inside the lot
    ///     satisfies every authored edge condition (or the payload doesn't fit). A value
    ///     <c>≥ 1</c> means placement is allowed; every confirmed match against concrete
    ///     in-room terrain contributes <see cref="EdgeMatchTieBreakerAlpha" /> as a
    ///     tie-breaker bias.
    /// </summary>
    /// <remarks>
    ///     Anchor selection is described in detail at <see cref="TryComputeAnchor" />. When
    ///     <see cref="LotPlacementContext.AnchorCache" /> is set the computed anchor is stored so a
    ///     subsequent <see cref="TryPlaceStaticMap" /> for the winning fragment can reuse it.
    /// </remarks>
    public float GetEdgeMatchMultiplier(RoomComponent room, in LotPlacementContext ctx)
    {
        if (!TryComputeAnchor(room, ctx, random: null, out var anchor, out var matched))
        {
            return 0f;
        }

        ctx.AnchorCache?.Store(this, anchor);
        return 1f + matched * EdgeMatchTieBreakerAlpha;
    }

    /// <summary>
    ///     Multiplier applied to the base generation weight that favours fragments aligned
    ///     with the room's still-outstanding demand:
    ///     <list type="bullet">
    ///         <item>
    ///             For every placeable <see cref="RoomFeatures" /> bit set on the room but
    ///             not yet bound (<see cref="RoomComponent.BoundPlaceableFeatures" />): if
    ///             this fragment's <see cref="Tags" /> contain the canonical feature tag
    ///             from <see cref="FeatureTagNames" />, the weight is multiplied by
    ///             <see cref="DemandTagBonus" />. If <see cref="AntiTags" /> contain it,
    ///             the weight is divided by <see cref="DemandTagBonus" />.
    ///         </item>
    ///         <item>
    ///             For every non-placeable room-level bit in <see cref="NonPlaceableFeatures" />
    ///             set on the room: same bonus / penalty applied against the matching tag.
    ///         </item>
    ///         <item>
    ///             Fragments carrying the <see cref="WildcardFeatureTag" /> match every
    ///             outstanding placeable bit (a single bonus is applied per outstanding
    ///             bit they don't already match directly). The wildcard does not cover
    ///             non-placeable room-level features.
    ///         </item>
    ///         <item>
    ///             For every cardinal direction in <see cref="LotPlacementContext.DemandedDoorways" />:
    ///             same bonus / penalty applied against the matching
    ///             <see cref="DoorDirectionTag" /> entry.
    ///         </item>
    ///     </list>
    ///     Demand and anti-tag matches share a single signed counter: each demand match
    ///     increments it and each anti-tag match decrements it. A non-negative net <c>n</c>
    ///     favours the fragment linearly by <c>1 + n * (DemandTagBonus - 1)</c>; a negative net
    ///     penalises it by the reciprocal <c>1 / (1 + |n| * (DemandTagBonus - 1))</c>, so a
    ///     single net demand match multiplies by <see cref="DemandTagBonus" /> and a single net
    ///     anti-tag match divides by it. Matches accumulate linearly instead of compounding as
    ///     <c>DemandTagBonus^n</c>. The undemanded-trap
    ///     penalty is applied once and stays multiplicative. <c>1</c> is returned when no demand
    ///     is outstanding or the fragment carries no relevant tags / anti-tags.
    /// </summary>
    public float GetDemandMultiplier(RoomComponent room, in LotPlacementContext ctx)
    {
        var multiplier = 1f;
        var matches = 0;
        var tags = Tags;
        var antiTags = AntiTags;
        var hasWildcard = tags is not null && tags.Contains(WildcardFeatureTag);

        // Hard-exclusion pass: if the fragment's tags advertise a placeable feature
        // (e.g. `small-fountain` from a `{` glyph) that the room didn't demand, the
        // fragment must not be picked — placing it would saddle the room with themed
        // contents it never asked for. Wildcard-only fragments carry no specific
        // feature tag and pass through unaffected; fragments that exclusively advertise
        // non-placeable capabilities (e.g. `entrance`, which isn't in
        // <see cref="FeatureGlyphs" />) are likewise unaffected. Trapped is exempt:
        // traps overlay floor without claiming the room's theme, so trap-bearing
        // fragments are allowed anywhere but receive <see cref="UndemandedTrapPenalty" />
        // when the room didn't ask for them.
        if (tags is not null)
        {
            foreach (var bit in PlaceableFeatureBits)
            {
                if (bit == RoomFeatures.Trapped)
                {
                    if ((room.Features & RoomFeatures.Trapped) == 0
                        && FeatureTagNames.TryGetValue(RoomFeatures.Trapped, out var trappedTag)
                        && tags.Contains(trappedTag))
                    {
                        multiplier *= UndemandedTrapPenalty;
                    }
                    continue;
                }

                if ((room.Features & bit) == 0
                    && FeatureTagNames.TryGetValue(bit, out var tagName)
                    && tags.Contains(tagName))
                {
                    return 0f;
                }
            }
        }

        var outstandingPlaceable = room.Features & PlaceableFeaturesMask & ~room.BoundPlaceableFeatures;
        var outstandingNonPlaceable = room.Features & NonPlaceableFeatures;
        var outstanding = outstandingPlaceable | outstandingNonPlaceable;
        if (outstanding != 0)
        {
            foreach (var (bit, tagName) in FeatureTagNames)
            {
                if ((outstanding & bit) != bit)
                {
                    continue;
                }

                var hasTag = tags is not null && tags.Contains(tagName);
                var wildcardApplies = hasWildcard && (outstandingPlaceable & bit) == bit;
                if (hasTag || wildcardApplies)
                {
                    matches++;
                }
                if (antiTags is not null && antiTags.Contains(tagName))
                {
                    matches--;
                }
            }
        }

        if (ctx.DemandedDoorways != DirectionFlags.None)
        {
            ApplyDoorDirectionDemand(ctx.DemandedDoorways, DirectionFlags.North, Direction.North, ref matches, tags, antiTags);
            ApplyDoorDirectionDemand(ctx.DemandedDoorways, DirectionFlags.South, Direction.South, ref matches, tags, antiTags);
            ApplyDoorDirectionDemand(ctx.DemandedDoorways, DirectionFlags.East, Direction.East, ref matches, tags, antiTags);
            ApplyDoorDirectionDemand(ctx.DemandedDoorways, DirectionFlags.West, Direction.West, ref matches, tags, antiTags);
        }

        // Fast path: most fragments match no outstanding demand, so skip the arithmetic.
        if (matches == 0)
        {
            return multiplier;
        }

        // Demand and anti-tag matches are unified into a single signed counter: each demand
        // match increments it, each anti-tag match decrements it. A positive net favours the
        // fragment linearly (1 + n * (DemandTagBonus - 1)); a negative net penalises it by the
        // reciprocal (1 / (1 + |n| * (DemandTagBonus - 1))), so a single net demand multiplies
        // by DemandTagBonus and a single net anti-tag divides by it. The once-applied
        // undemanded-trap penalty stays in `multiplier`.
        return matches > 0
            ? multiplier * (1f + matches * (DemandTagBonus - 1f))
            : multiplier / (1f - matches * (DemandTagBonus - 1f));
    }

    private static void ApplyDoorDirectionDemand(
        DirectionFlags demanded, DirectionFlags flag, Direction direction,
        ref int matches, ICollection<string>? tags, ICollection<string>? antiTags)
    {
        if ((demanded & flag) != flag)
        {
            return;
        }

        var tagName = DoorDirectionTag[direction];
        if (tags is not null && tags.Contains(tagName))
        {
            matches++;
        }
        if (antiTags is not null && antiTags.Contains(tagName))
        {
            matches--;
        }
    }

    /// <summary>
    ///     Searches the candidate anchor positions inside <paramref name="ctx" />'s lot for
    ///     a feasible placement that maximises (tightness, matched). Returns the room-space
    ///     top-left of the fragment's payload.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <b>Step 1 — Anchor range (geometric only):</b> per axis, if
    ///         <c>payload.dim == lot.dim</c> the anchor is pinned to that edge; otherwise
    ///         the anchor sweeps the lot. <see cref="LotPlacementContext.TargetEdge" />
    ///         does <em>not</em> pin the range — it feeds the tightness score below as a
    ///         soft preference.
    ///     </para>
    ///     <para>
    ///         <b>Step 2 — Score candidates (perimeter-only sweep):</b> only the outer
    ///         2-cell ring of the anchor-range rectangle is visited. Anchors deeper than
    ///         1 cell can never sit flush against any side of the lot and add no new
    ///         match potential beyond what the 1-deep ring already provides, so they are
    ///         skipped. For each visited <c>(x, y)</c>:
    ///         <list type="bullet">
    ///             <item><c>matched</c>: number of confirmed in-room condition matches.</item>
    ///             <item><c>tightness</c>: 1 when the anchor sits flush against
    ///                 <see cref="LotPlacementContext.TargetEdge" /> (or any lot edge when
    ///                 <c>TargetEdge</c> is null), else 0.</item>
    ///         </list>
    ///         The pick order is (tightness desc, matched desc); ties are broken
    ///         uniformly via reservoir sampling on <paramref name="random" />.
    ///         <c>TargetEdge</c> is honoured when feasible and silently overridden when
    ///         no flush anchor satisfies its conditions.
    ///     </para>
    ///     <para>
    ///         <b>Step 3 — Per-slot evaluation</b> classifies each conditional slot's
    ///         neighbor cell as (A) out-of-room, (B) in-room already filled, or (C)
    ///         in-room still empty and applies the rules described on
    ///         <see cref="EvaluateEdgeAtAnchor" />.
    ///     </para>
    /// </remarks>
    public bool TryComputeAnchor(
        RoomComponent room, in LotPlacementContext ctx,
        SimpleRandom? random, out Point anchor, out int matched)
    {
        anchor = default;
        matched = 0;

        var lot = ctx.Lot;
        if (PayloadArea.Width > lot.Width || PayloadArea.Height > lot.Height)
        {
            return false;
        }

        var minX = lot.TopLeft.X;
        var maxX = lot.BottomRight.X - PayloadArea.Width + 1;
        var minY = lot.TopLeft.Y;
        var maxY = lot.BottomRight.Y - PayloadArea.Height + 1;

        // Visit the outer 2-ring of the anchor-range rectangle first: that's where
        // every flush-to-lot-edge (tightness == 1) anchor lives and where most
        // "slide one cell off the wall" deferred placements land. If the rim yields
        // no feasible candidate, fall back to the interior so a fragment whose
        // conditional edges only line up against deep in-room neighbors still has
        // a chance to place. The two-pass split keeps the common case cheap (a 5×5
        // payload in a 20×16 lot has 16×12 = 192 anchors; the rim is 80) while
        // preserving completeness.
        const int RimDepth = 2;

        var bestAnchor = default(Point);
        var bestTightness = -1;
        var bestMatched = -1;
        var tieCount = 0;

        for (var y = minY; y <= maxY; y++)
        {
            var rowInRim = y - minY < RimDepth || maxY - y < RimDepth;
            for (var x = minX; x <= maxX; x++)
            {
                var colInRim = x - minX < RimDepth || maxX - x < RimDepth;
                if (!rowInRim && !colInRim)
                {
                    continue;
                }

                ScoreCandidate(new Point((byte)x, (byte)y), room, ctx, random,
                    ref bestAnchor, ref bestTightness, ref bestMatched, ref tieCount);
            }
        }

        if (bestMatched < 0)
        {
            // Rim pass found nothing — sweep the interior. Anchors here can only
            // score tightness == 0, but their (B)-class matches against deep
            // in-room terrain may be the only way to satisfy this fragment's
            // conditional edges.
            for (var y = minY + RimDepth; y <= maxY - RimDepth; y++)
            {
                for (var x = minX + RimDepth; x <= maxX - RimDepth; x++)
                {
                    ScoreCandidate(new Point((byte)x, (byte)y), room, ctx, random,
                        ref bestAnchor, ref bestTightness, ref bestMatched, ref tieCount);
                }
            }
        }

        if (bestMatched < 0)
        {
            return false;
        }

        anchor = bestAnchor;
        matched = bestMatched;
        return true;
    }

    private void ScoreCandidate(
        Point candidate, RoomComponent room, in LotPlacementContext ctx, SimpleRandom? random,
        ref Point bestAnchor, ref int bestTightness, ref int bestMatched, ref int tieCount)
    {
        if (!TryEvaluateAnchor(candidate, room, ctx, out var candidateMatched))
        {
            return;
        }

        var tightness = ComputeTightness(candidate, ctx);
        if (tightness > bestTightness
            || (tightness == bestTightness && candidateMatched > bestMatched))
        {
            bestTightness = tightness;
            bestMatched = candidateMatched;
            bestAnchor = candidate;
            tieCount = 1;
        }
        else if (tightness == bestTightness && candidateMatched == bestMatched)
        {
            tieCount++;
            // Reservoir sampling for uniform tie-breaking across equally-scored anchors.
            if (random != null && random.Next(tieCount) == 0)
            {
                bestAnchor = candidate;
            }
        }
    }

    /// <summary>
    ///     Tightness preference: 1 when <paramref name="anchor" /> places the payload flush
    ///     against <see cref="LotPlacementContext.TargetEdge" /> (or against any lot edge
    ///     when no target edge was specified), otherwise 0.
    /// </summary>
    private int ComputeTightness(Point anchor, in LotPlacementContext ctx)
    {
        var lot = ctx.Lot;
        switch (ctx.TargetEdge)
        {
            case Direction.North:
                return anchor.Y == lot.TopLeft.Y ? 1 : 0;
            case Direction.South:
                return anchor.Y + PayloadArea.Height - 1 == lot.BottomRight.Y ? 1 : 0;
            case Direction.West:
                return anchor.X == lot.TopLeft.X ? 1 : 0;
            case Direction.East:
                return anchor.X + PayloadArea.Width - 1 == lot.BottomRight.X ? 1 : 0;
            case null:
                var flush = anchor.Y == lot.TopLeft.Y
                    || anchor.X == lot.TopLeft.X
                    || anchor.Y + PayloadArea.Height - 1 == lot.BottomRight.Y
                    || anchor.X + PayloadArea.Width - 1 == lot.BottomRight.X;
                return flush ? 1 : 0;
            default:
                return 0;
        }
    }

    private bool TryEvaluateAnchor(
        Point anchor, RoomComponent room, in LotPlacementContext ctx, out int matched)
    {
        matched = 0;
        if (!EvaluateEdgeAtAnchor(Direction.North, anchor, room, ctx, ref matched))
        {
            return false;
        }

        if (!EvaluateEdgeAtAnchor(Direction.South, anchor, room, ctx, ref matched))
        {
            return false;
        }

        if (!EvaluateEdgeAtAnchor(Direction.West, anchor, room, ctx, ref matched))
        {
            return false;
        }

        if (!EvaluateEdgeAtAnchor(Direction.East, anchor, room, ctx, ref matched))
        {
            return false;
        }

        if (!TryEvaluatePreservedConditions(anchor, ctx, ref matched))
        {
            return false;
        }

        // Payload overlap with previously placed terrain is only possible when the room
        // layout shifted the lot onto an adjacent placed chunk (DenseRoomLayout's
        // wall-merge). Otherwise the lot is disjoint from every placed chunk and the
        // payload writes are guaranteed to land on empty cells.
        if (ctx.LotShifted && !EvaluatePayloadOverlapCompatibility(anchor, room, ctx))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Validates each payload cell against the room's existing terrain at the
    ///     anchored position. Cells over <see cref="MapFeature.Default" /> (empty space)
    ///     or carrying the identical feature are always compatible. Cells over a
    ///     previously placed neighbor (typical after <c>DenseRoomLayout.TryShiftToMerge</c>)
    ///     are rejected only when both the existing and incoming features are
    ///     <see cref="IsImportantCell">important</see> and differ &#8212;
    ///     the non-important side always yields. Out-of-bounds cells fail.
    /// </summary>
    private bool EvaluatePayloadOverlapCompatibility(Point anchor, RoomComponent room, in LotPlacementContext ctx)
    {
        // Dynamic fragments use a zero-length ByteMap (set by EnsureInitialized when Map
        // is empty) and place generic fill that never carries important features, so
        // overlap with existing important content is fine.
        if (ByteMap!.Length == 0)
        {
            return true;
        }

        var area = PayloadArea;
        var originX = area.TopLeft.X;
        var originY = area.TopLeft.Y;
        for (var ly = originY; ly <= area.BottomRight.Y; ly++)
        {
            for (var lx = originX; lx <= area.BottomRight.X; lx++)
            {
                var roomX = anchor.X + (lx - originX);
                var roomY = anchor.Y + (ly - originY);
                if (roomX < 0 || roomY < 0 || roomX >= room.Width || roomY >= room.Height)
                {
                    return false;
                }

                var existingFeature = room.Terrain[room.PointToIndex![roomX, roomY]];
                // Background default-terrain (the fill from AllocateTerrain) is not
                // "placed by a neighbor" — only cells that differ from the room's
                // default terrain represent actual chunk content we might overlap with.
                if (existingFeature == ctx.DefaultTerrain)
                {
                    continue;
                }

                var glyph = (char)ByteMap[PointToIndex![lx, ly]];
                var fragmentFeature = GlyphToFeature(glyph);
                if (fragmentFeature == existingFeature)
                {
                    continue;
                }

                var roomPoint = new Point((byte)roomX, (byte)roomY);
                var existingImportant = IsImportantCell(room, roomPoint);
                var fragmentImportant = IsImportantFeature(fragmentFeature)
                    || glyph == '<' || glyph == '>';
                if (existingImportant && fragmentImportant)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsImportantFeature(MapFeature feature)
        => feature is not (MapFeature.Default
            or MapFeature.StoneWall or MapFeature.RockWall
            or MapFeature.StoneFloor or MapFeature.RockFloor);

    /// <summary>
    ///     Evaluates this fragment's own <see cref="GetEdge" /> array in
    ///     <paramref name="direction" /> against the neighbor cells
    ///     <paramref name="anchor" /> would place each payload edge tile against.
    ///     Each non-<see cref="EdgeConditionKind.Default" /> slot is classified by its
    ///     neighbor cell:
    ///     <list type="bullet">
    ///         <item>
    ///             <b>(A) Out-of-room:</b> <see cref="EdgeConditionKind.Edge" />,
    ///             <see cref="EdgeConditionKind.Unfilled" /> and
    ///             <see cref="EdgeConditionKind.Default" /> match;
    ///             <see cref="EdgeConditionKind.Filled" />,
    ///             <see cref="EdgeConditionKind.Doorway" /> and tile-specific conditions
    ///             fail. Authors who want the fragment to dock against the room boundary
    ///             use <c>E</c>; this is what makes open-perimeter fragments reject
    ///             boundary placement.
    ///         </item>
    ///         <item>
    ///             <b>(B) In-room and already filled:</b> evaluated against the actual
    ///             <see cref="MapFeature" /> via <see cref="EvaluateCondition" />.
    ///             Counted into <paramref name="matched" /> on success.
    ///         </item>
    ///         <item>
    ///             <b>(C) In-room and still empty:</b> the condition is deferred to a
    ///             future chunk. We additionally check that the new condition is
    ///             compatible with any preserved conditions already registered on the
    ///             same cell — if a later fragment couldn't satisfy them all
    ///             simultaneously the placement is rejected here so the contradiction
    ///             never reaches <see cref="FixupDeferredConditions" />. Deferred slots
    ///             do not contribute to <paramref name="matched" />; only confirmed (B)
    ///             matches do.
    ///         </item>
    ///     </list>
    /// </summary>
    private bool EvaluateEdgeAtAnchor(
        Direction direction, Point anchor, RoomComponent room, in LotPlacementContext ctx, ref int matched)
    {
        var edge = GetEdge(direction);
        if (edge == null)
        {
            return true;
        }

        var horizontal = direction is Direction.North or Direction.South;
        var slotCount = horizontal ? PayloadArea.Width : PayloadArea.Height;

        for (var slotIndex = 0; slotIndex < slotCount; slotIndex++)
        {
            var condition = edge[slotIndex];
            if (condition == EdgeConditionKind.Default)
            {
                continue;
            }

            int neighborX, neighborY;
            if (horizontal)
            {
                neighborX = anchor.X + slotIndex;
                neighborY = direction == Direction.North
                    ? anchor.Y - 1
                    : anchor.Y + PayloadArea.Height;
            }
            else
            {
                neighborY = anchor.Y + slotIndex;
                neighborX = direction == Direction.West
                    ? anchor.X - 1
                    : anchor.X + PayloadArea.Width;
            }

            var neighborInRoom = neighborX >= 0 && neighborY >= 0
                && neighborX < room.Width && neighborY < room.Height;

            if (!neighborInRoom)
            {
                // (A) Out-of-room. Edge / Unfilled match; anything else fails.
                // Default is filtered at the top of the loop so it never lands here.
                switch (condition)
                {
                    case EdgeConditionKind.Edge:
                    case EdgeConditionKind.Unfilled:
                        matched++;
                        continue;
                    default:
                        return false;
                }
            }

            var neighborPoint = new Point((byte)neighborX, (byte)neighborY);
            var neighborFeature = room.Terrain[room.PointToIndex![neighborX, neighborY]];

            // (B) Evaluate the condition against the actual terrain at the neighbor
            // cell — works whether the cell carries a chunk-written feature or the
            // room's background fill.
            if (EvaluateCondition(condition, constrained: neighborFeature,
                    neighborInRoom: true, ref matched))
            {
                continue;
            }

            // (C) Condition didn't match concrete terrain. If the cell is still
            // background fill AND the partition tracks it as allocatable to a future
            // chunk, defer — the unmet condition is carried forward by
            // RegisterPreservedConditions and re-evaluated against whichever chunk
            // lands on the cell next. Otherwise the condition can never be satisfied
            // and we fail.
            if (neighborFeature != ctx.DefaultTerrain
                || ctx.EmptySpace == null
                || !ctx.EmptySpace.Contains(neighborPoint))
            {
                return false;
            }

            // A condition that can't be satisfied by a wall (e.g. StoneFloor / Doorway)
            // forces FixupDeferredConditions to fill the neighbor cell with walkable
            // terrain. That floor could be exposed to unfilled cells unless a
            // wall can be placed directly behind it (one cell further out). Reserve that
            // space now: reject the placement unless the cell two out — in this edge's
            // outward direction — is still in-room, leaving a 2-cell gap between this
            // edge and the room border.
            if (RequiresNonWallFill(condition))
            {
                var (outwardX, outwardY) = direction switch
                {
                    Direction.North => (0, -1),
                    Direction.South => (0, 1),
                    Direction.West => (-1, 0),
                    _ => (1, 0)
                };
                var behindX = neighborX + outwardX;
                var behindY = neighborY + outwardY;
                if (behindX < 0 || behindY < 0 || behindX >= room.Width || behindY >= room.Height)
                {
                    return false;
                }
            }

            // The condition is deferrable. Verify it remains compatible with any
            // preserved conditions already registered on the cell — if a later
            // fragment couldn't satisfy them all simultaneously, reject now so the
            // contradiction never reaches FixupDeferredConditions.
            if (!IsConditionCompatibleWithPreserved(condition, neighborPoint, ctx))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Whether a deferred <paramref name="condition" /> can only be satisfied by a
    ///     <em>walkable</em> (non-wall) feature, so <see cref="FixupDeferredConditions" />
    ///     must fill the cell with floor rather than a wall. These are the conditions that
    ///     drive the 2-cell boundary gap (in <see cref="EvaluateEdgeAtAnchor" />) and the
    ///     StoneWall buffer condition seeded behind them (in <see cref="RegisterPreservedConditions" />):
    ///     <see cref="EdgeConditionKind.Doorway" /> and every tile-specific condition whose
    ///     <see cref="MapFeature" /> is not a wall. <see cref="EdgeConditionKind.Filled" />
    ///     accepts a wall, and <see cref="EdgeConditionKind.Unfilled" /> /
    ///     <see cref="EdgeConditionKind.Edge" /> ask the cell to stay empty, so none of
    ///     those force a floor fill.
    /// </summary>
    private static bool RequiresNonWallFill(EdgeConditionKind condition)
        => condition switch
        {
            EdgeConditionKind.Default
                or EdgeConditionKind.Filled
                or EdgeConditionKind.Unfilled
                or EdgeConditionKind.Edge 
                or EdgeConditionKind.Doorway => false,
            _ => !IsWallFeature((MapFeature)condition)
        };

    /// <summary>
    ///     Returns <c>true</c> when adding <paramref name="condition" /> to any conditions
    ///     already preserved on <paramref name="cell" /> (via
    ///     <see cref="LotPlacementContext.PreservedConditions" />) still admits at least
    ///     one satisfying <see cref="MapFeature" /> (or every entry asks the cell to stay
    ///     empty). This is the placement-time compatibility check that prevents two
    ///     adjacent chunks from registering contradictory deferred conditions on the same
    ///     empty cell.
    /// </summary>
    private static bool IsConditionCompatibleWithPreserved(
        EdgeConditionKind condition, Point cell, in LotPlacementContext ctx)
    {
        var preserved = ctx.PreservedConditions;
        if (preserved == null || !preserved.TryGetValue(cell, out var existing) || existing.Length == 0)
        {
            return true;
        }

        // Merge existing + the new condition on the stack (the set is tiny and deduplicated) and
        // run the same intersection logic the fixup pass uses.
        Span<EdgeConditionKind> merged = stackalloc EdgeConditionKind[existing.Length + 1];
        existing.CopyTo(merged);
        merged[^1] = condition;

        // Reuse a per-thread scratch list instead of allocating a List<MapFeature> per call:
        // this is a hot placement-time check (one call per deferred edge slot per anchor
        // candidate). The list is fully consumed before this method returns and the method
        // never re-enters itself, so a single instance is safe; thread-static keeps parallel
        // level generation race-free.
        var candidates = _compatibilityCandidatesScratch ??= [];
        candidates.Clear();
        // TryCollectSatisfyingFeatures returns false in two cases — both acceptable
        // here: (i) any Unfilled/Edge entry, in which case all entries must agree that
        // the cell stays empty; (ii) positive constraints intersect to the empty set,
        // which is the contradiction we want to reject.
        if (TryCollectSatisfyingFeatures(merged, candidates))
        {
            return true;
        }

        // Unfilled/Edge-only is compatible: every entry asks the cell to stay empty.
        for (var i = 0; i < merged.Length; i++)
        {
            if (merged[i] is not (EdgeConditionKind.Unfilled
                or EdgeConditionKind.Edge
                or EdgeConditionKind.Default))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    ///     Walks every payload cell at <paramref name="anchor" />, looks up the cell in
    ///     <see cref="LotPlacementContext.PreservedConditions" />, and verifies that this
    ///     fragment's tile at the cell satisfies every condition contributed by adjacent
    ///     placed chunks.
    /// </summary>
    private bool TryEvaluatePreservedConditions(
        Point anchor, in LotPlacementContext ctx, ref int matched)
    {
        var preservedConditions = ctx.PreservedConditions;
        if (preservedConditions == null || preservedConditions.Count == 0)
        {
            return true;
        }

        var payloadOrigin = PayloadArea.TopLeft;
        for (var dy = 0; dy < PayloadArea.Height; dy++)
        {
            for (var dx = 0; dx < PayloadArea.Width; dx++)
            {
                var cell = new Point((byte)(anchor.X + dx), (byte)(anchor.Y + dy));
                if (!preservedConditions.TryGetValue(cell, out var conditionsAtCell))
                {
                    continue;
                }

                var fragmentFeature = ByteMap!.Length == 0
                    ? MapFeature.Default
                    : GlyphToFeature((char)ByteMap![PointToIndex![
                        payloadOrigin.X + dx, payloadOrigin.Y + dy]]);

                for (var conditionIndex = 0; conditionIndex < conditionsAtCell.Length; conditionIndex++)
                {
                    var condition = conditionsAtCell[conditionIndex];
                    if (!EvaluateCondition(condition, constrained: fragmentFeature,
                            neighborInRoom: true, ref matched))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     Evaluates an edge condition against a given map feature.
    /// </summary>
    public static bool EvaluateCondition(
        EdgeConditionKind condition, MapFeature constrained,
        bool neighborInRoom, ref int matched)
    {
        switch (condition)
        {
            case EdgeConditionKind.Default:
                return true;
            case EdgeConditionKind.Unfilled:
                if (constrained != MapFeature.Default)
                {
                    return false;
                }
                break;
            case EdgeConditionKind.Filled:
                if (constrained == MapFeature.Default)
                {
                    return false;
                }
                break;
            case EdgeConditionKind.Edge:
                if (neighborInRoom)
                {
                    return false;
                }
                break;
            case EdgeConditionKind.Doorway:
                if (constrained != MapFeature.StoneFloor && constrained != MapFeature.StoneArchway)
                {
                    return false;
                }
                break;
            default:
                // Tile-specific: numeric value equals the required MapFeature.
                if (constrained != (MapFeature)condition)
                {
                    return false;
                }
                break;
        }

        matched++;
        return true;
    }

    /// <summary>
    ///     Registers <paramref name="chunk" />'s edge conditions onto cells immediately
    ///     outside the chunk, into <paramref name="preservedConditions" />. Only conditions
    ///     whose corresponding outside cell is still tracked as empty in
    ///     <paramref name="emptySpace" /> are kept — cells already covered by another
    ///     chunk are no longer up for grabs. A given outside cell may accumulate
    ///     conditions from multiple chunks.
    /// </summary>
    public static void RegisterPreservedConditions(
        Chunk chunk,
        RoomComponent room,
        RectanglePartition emptySpace,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters)
    {
        ArgumentNullException.ThrowIfNull(emptySpace);
        var fragment = chunk.Fragment;
        var bounds = chunk.BoundingRectangle;
        RegisterPreservedEdge(fragment.NorthEdge, Direction.North, bounds,
            room, emptySpace, preservedConditions);
        RegisterPreservedEdge(fragment.SouthEdge, Direction.South, bounds,
            room, emptySpace, preservedConditions);
        RegisterPreservedEdge(fragment.WestEdge, Direction.West, bounds,
            room, emptySpace, preservedConditions);
        RegisterPreservedEdge(fragment.EastEdge, Direction.East, bounds,
            room, emptySpace, preservedConditions);
    }

    // Shared, immutable single-element preserved-condition arrays keyed by the byte enum value,
    // so the common case (a cell carrying exactly one condition) reuses a cached array instead of
    // allocating a List<EdgeConditionKind> + its backing array. Multi-condition cells (rare) get a
    // freshly grown array. Cached entries are never mutated — RegisterPreservedEdge replaces the
    // dictionary value with a new array when a second distinct condition arrives. Sized to the
    // compacted EdgeConditionKind range (Doorway is the highest value).
    private static readonly EdgeConditionKind[]?[] _singleConditionArrays =
        new EdgeConditionKind[(int)EdgeConditionKind.Doorway + 1][];

    // Per-thread scratch list reused by IsConditionCompatibleWithPreserved to avoid a
    // List<MapFeature> allocation on every placement-time compatibility check. Thread-static
    // because level generation may run concurrently across threads.
    [ThreadStatic]
    private static List<MapFeature>? _compatibilityCandidatesScratch;

    private static EdgeConditionKind[] GetSingleConditionArray(EdgeConditionKind condition)
        => _singleConditionArrays[(byte)condition] ??= [condition];

    private static void RegisterPreservedEdge(
        EdgeConditionKind[]? edge,
        Direction edgeDirection,
        Rectangle chunkBounds,
        RoomComponent room,
        RectanglePartition emptySpace,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions)
    {
        if (edge == null)
        {
            return;
        }

        for (var slotIndex = 0; slotIndex < edge.Length; slotIndex++)
        {
            var condition = edge[slotIndex];
            if (condition == EdgeConditionKind.Default
                || condition == EdgeConditionKind.Edge)
            {
                // Default has nothing to preserve; Edge is a self-constraint on the
                // fragment's own edge tile, not a requirement that transfers to the
                // neighbor cell.
                continue;
            }

            int outsideX, outsideY;
            switch (edgeDirection)
            {
                case Direction.North:
                    outsideX = chunkBounds.TopLeft.X + slotIndex;
                    outsideY = chunkBounds.TopLeft.Y - 1;
                    break;
                case Direction.South:
                    outsideX = chunkBounds.TopLeft.X + slotIndex;
                    outsideY = chunkBounds.BottomRight.Y + 1;
                    break;
                case Direction.West:
                    outsideX = chunkBounds.TopLeft.X - 1;
                    outsideY = chunkBounds.TopLeft.Y + slotIndex;
                    break;
                case Direction.East:
                    outsideX = chunkBounds.BottomRight.X + 1;
                    outsideY = chunkBounds.TopLeft.Y + slotIndex;
                    break;
                default:
                    continue;
            }

            var outsideInRoom = outsideX >= 0 && outsideY >= 0
                && outsideX < room.Width && outsideY < room.Height;
            if (!outsideInRoom)
            {
                continue;
            }

            var outsidePoint = new Point((byte)outsideX, (byte)outsideY);
            if (!emptySpace.Contains(outsidePoint))
            {
                continue;
            }

            if (!preservedConditions.TryGetValue(outsidePoint, out var conditionsAtCell))
            {
                // First condition on this cell: store the shared cached single-element array.
                preservedConditions[outsidePoint] = GetSingleConditionArray(condition);
            }
            else if (Array.IndexOf(conditionsAtCell, condition) < 0)
            {
                // Second+ distinct condition (rare): grow into a fresh owned array. Deduplicate
                // — an identical condition repeated by an adjacent chunk constrains nothing further.
                var grown = new EdgeConditionKind[conditionsAtCell.Length + 1];
                Array.Copy(conditionsAtCell, grown, conditionsAtCell.Length);
                grown[^1] = condition;
                preservedConditions[outsidePoint] = grown;
            }

            // A condition that can only be satisfied by walkable terrain forces
            // FixupDeferredConditions to fill this outside cell with floor. Register a
            // StoneWall condition on the cell directly behind it (one step further out)
            // so the same fixup pass walls it off and the floor isn't exposed to the
            // unfilled cells. EvaluateEdgeAtAnchor's 2-cell gap check already
            // guaranteed that behind cell is in-room; only seed the wall where the cell
            // is still empty space and carries no condition of its own.
            if (RequiresNonWallFill(condition))
            {
                var (behindDx, behindDy) = edgeDirection switch
                {
                    Direction.North => (0, -1),
                    Direction.South => (0, 1),
                    Direction.West => (-1, 0),
                    _ => (1, 0)
                };
                var behindX = outsideX + behindDx;
                var behindY = outsideY + behindDy;
                if (behindX >= 0 && behindY >= 0 && behindX < room.Width && behindY < room.Height)
                {
                    var behindPoint = new Point((byte)behindX, (byte)behindY);
                    if (emptySpace.Contains(behindPoint) && !preservedConditions.ContainsKey(behindPoint))
                    {
                        preservedConditions[behindPoint] = GetSingleConditionArray(EdgeConditionKind.StoneWall);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Fixup pass run once <see cref="UniformRoomLayout.PlaceSurroundingFragments" />
    ///     can place no more fragments. For every still-empty cell in
    ///     <paramref name="preservedConditions" />, picks a <see cref="MapFeature" /> that
    ///     satisfies every preserved condition recorded for that cell and writes it via
    ///     <see cref="TerrainSystem.SetTerrain" />. The filled cells are then grouped
    ///     into maximal axis-aligned rectangles and emitted as synthetic chunks with
    ///     <see cref="NormalMapFragmentData.Deferred" /> as their fragment.
    ///     <para>
    ///         Cells whose preserved conditions can only be honoured by leaving the cell
    ///         empty (any <see cref="EdgeConditionKind.Unfilled" /> or
    ///         <see cref="EdgeConditionKind.Edge" /> entry) are skipped: they stay as
    ///         <see cref="MapFeature.Default" /> and don't contribute a chunk.
    ///     </para>
    ///     <para>
    ///         Cells whose conditions contradict each other (e.g. two disjoint
    ///         tile-specific demands) cannot reach this pass: the placement-time
    ///         compatibility check in <see cref="EvaluateEdgeAtAnchor" /> rejects any
    ///         placement that would register a conflicting deferred condition on a
    ///         cell that already carries one.
    ///     </para>
    ///     <para>
    ///         Selection preference when multiple features satisfy the conditions:
    ///         (1) a wall (<see cref="MapFeature.StoneWall" /> preferred over
    ///         <see cref="MapFeature.RockWall" />); (2) a feature already carried by one
    ///         of the four cardinal neighbors; (3) a uniformly random satisfying feature.
    ///     </para>
    ///     <para>
    ///         TODO: close these cells off with walls more intelligently — e.g. only
    ///         place a wall when at least one neighbor is already a wall so the new
    ///         cells extend an existing wall line, otherwise prefer the dominant
    ///         neighbor feature to keep the room visually contiguous. The current pass
    ///         is the minimum needed to keep deferred conditions from leaving raw
    ///         <see cref="MapFeature.Default" /> holes in the level.
    ///     </para>
    /// </summary>
    public static List<Chunk> FixupDeferredConditions(
        RoomComponent room,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        SimpleRandom random,
        RoomParameters roomParameters)
    {
        if (preservedConditions.Count == 0)
        {
            return [];
        }

        var filled = new List<Point>();
        var candidates = new List<MapFeature>();
        foreach (var (point, conditions) in preservedConditions)
        {
            if (room.Terrain[room.PointToIndex![point.X, point.Y]] != roomParameters.DefaultTerrain)
            {
                // A later chunk landed on this cell (or the cell was filled by an
                // earlier fixup neighbor); the preserved conditions were already
                // evaluated against the chunk's payload during TryPlace.
                continue;
            }

            candidates.Clear();
            if (!TryCollectSatisfyingFeatures(conditions, candidates))
            {
                continue;
            }

            var picked = PickPreferredFixupFeature(candidates, point, room, random, roomParameters.DefaultWall);
            TerrainSystem.SetTerrain(picked, point, room);
            filled.Add(point);
        }

        // Perimeter enclosure: a cell filled with walkable terrain (its conditions
        // forbade a wall) exposes floor to the unfilled cells on any side the
        // chunk's edge conditions didn't reach — typically near the chunk corners. The
        // cell directly outward is already covered by a StoneWall buffer condition
        // seeded in RegisterPreservedEdge; this seals the remaining unfilled cells
        // around every walkable condition cell.
        WallAroundConditionCells(room, preservedConditions, roomParameters, filled);

        // Connectivity rescue: deferred-fill cells whose only neighbors are walls
        // (no real-chunk floor adjacent) become isolated walkable pockets that the
        // corridor system can't connect, because deferred-fill chunks author no
        // doorways. Identify the connected component of walkable cells reachable
        // from any non-fixup floor and demote unreachable fixup floors to walls.
        // This keeps the cleanup narrowly scoped to fixup cells — authored chunk
        // interiors are never walled off, only the synthesized fill that has no
        // way to be connected.
        ConvertIsolatedFixupCellsToWalls(room, filled, roomParameters);

        return GroupFilledCellsIntoChunks(room, filled, roomParameters);
    }

    /// <summary>
    ///     Seals the unfilled cells around every walkable cell that the fixup
    ///     itself filled. Edge conditions only constrain the single cell directly outside a
    ///     chunk edge, so a cell filled with walkable terrain can still face background on
    ///     the sides its edge didn't reach — typically near the chunk corners. For each such
    ///     fill, this walls every surrounding cell (8-directional) that is still
    ///     <see cref="RoomParameters.DefaultTerrain" /> and carries no preserved condition of
    ///     its own (a cell with its own condition is resolved by the fixup and must not be
    ///     pre-sealed). Only cells the fixup filled are considered — terrain a real chunk
    ///     placed is enclosed by that chunk's own structure. Newly walled cells are appended
    ///     to <paramref name="filled" /> so the chunk grouping owns them.
    /// </summary>
    private static void WallAroundConditionCells(
        RoomComponent room,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters,
        List<Point> filled)
    {
        var pointToIndex = room.PointToIndex!;
        // Snapshot the count: WallBackgroundNeighbor appends the sealing walls to `filled`,
        // and those walls must not themselves spawn another ring of sealing.
        var filledCount = filled.Count;
        for (var i = 0; i < filledCount; i++)
        {
            var point = filled[i];
            if (!room.Terrain[pointToIndex[point.X, point.Y]].CanMoveTo())
            {
                continue;
            }

            // Match an existing adjacent wall so the seal extends the surrounding wall line
            // instead of introducing a foreign wall type.
            var wall = RoomLayout.PickAdjacentWall(room, point, roomParameters.DefaultWall);

            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = Vector.MovementDirections[directionIndex];
                WallOffNeighbor(room, point.X + direction.X, point.Y + direction.Y,
                    preservedConditions, roomParameters, wall, filled);
            }
        }
    }

    private static void WallOffNeighbor(
        RoomComponent room, int x, int y,
        Dictionary<Point, EdgeConditionKind[]> preservedConditions,
        RoomParameters roomParameters,
        MapFeature wall,
        List<Point> filled)
    {
        if (x < 0 || y < 0 || x >= room.Width || y >= room.Height)
        {
            Debug.Assert(false, "Neighbor out of bounds");
            return;
        }

        if (room.Terrain[room.PointToIndex![x, y]] != roomParameters.DefaultTerrain)
        {
            // Already filled.
            return;
        }

        var cell = new Point((byte)x, (byte)y);
        if (preservedConditions.ContainsKey(cell))
        {
            // Has its own condition — the fixup may fill it with walkable terrain, so
            // sealing it here would be wrong.
            return;
        }

        TerrainSystem.SetTerrain(wall, cell, room);
        filled.Add(cell);
    }

    /// <summary>
    ///     Removes walkable cells from <paramref name="filledCells" /> (and converts the
    ///     room terrain at those cells to <see cref="MapFeature.StoneWall" />) when
    ///     they're unreachable from any walkable cell that wasn't placed by the fixup
    ///     pass. Preserves any fixup cell adjacent (8-directionally) to a real chunk's
    ///     floor / archway / pool — those are reachable through their neighbor's
    ///     authored doorway.
    /// </summary>
    private static void ConvertIsolatedFixupCellsToWalls(RoomComponent room, List<Point> filledCells, RoomParameters roomParameters)
    {
        if (filledCells.Count == 0)
        {
            return;
        }

        var pointToIndex = room.PointToIndex!;
        var tileCount = room.TileCount;
        // Rented bool[] indexed by room.PointToIndex serves as the "is this cell a
        // fixup cell?" lookup; cheaper than a HashSet<Point> for the inner BFS-seed
        // loop and avoids the per-cell hash + box-free struct comparison.
        var fixupMask = ArrayPool<bool>.Shared.Rent(tileCount);
        try
        {
            Array.Clear(fixupMask, 0, tileCount);
            for (var i = 0; i < filledCells.Count; i++)
            {
                var p = filledCells[i];
                fixupMask[pointToIndex[p.X, p.Y]] = true;
            }

            var rect = room.BoundingRectangle;

            // Seed BFS from every walkable cell that's OWNED by a real (non-fixup) chunk
            // — those are the cells the corridor system already routes to via the
            // authoring chunks' doorway points. Background default-terrain cells are
            // intentionally excluded as seeds so isolated fixup pockets surrounded by
            // background terrain are still detected.
            var reached = new HashSet<Point>();
            var queue = new Queue<Point>();
            var width = roomParameters.RoomWidth;
            var height = roomParameters.RoomHeight;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (roomParameters[x, y] is null)
                    {
                        continue;
                    }
                    if (fixupMask[pointToIndex[x, y]])
                    {
                        continue;
                    }
                    if (!room.Terrain[pointToIndex[x, y]].CanMoveTo())
                    {
                        continue;
                    }
                    var p = new Point((byte)x, (byte)y);
                    if (reached.Add(p))
                    {
                        queue.Enqueue(p);
                    }
                }
            }

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var neighbor in RoomLayout.Neighbors8(rect, p.X, p.Y))
                {
                    if (!room.Terrain[pointToIndex[neighbor.X, neighbor.Y]].CanMoveTo())
                    {
                        continue;
                    }

                    if (reached.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Any fixup cell still walkable but not reached is isolated — wall it off
            // and drop it from the chunk grouping so no $deferred chunk advertises an
            // unreachable interior.
            for (var i = filledCells.Count - 1; i >= 0; i--)
            {
                var p = filledCells[i];
                if (reached.Contains(p))
                {
                    continue;
                }
                var feature = room.Terrain[pointToIndex[p.X, p.Y]];
                if (!feature.CanMoveTo())
                {
                    // Fixup picked a wall already — keep it in `filled` so the chunk
                    // grouping still owns the cell, but no walling needed.
                    continue;
                }

                // Match an existing adjacent wall so the seal extends the surrounding wall
                // line instead of introducing a foreign wall type.
                var wall = RoomLayout.PickAdjacentWall(room, p, roomParameters.DefaultWall);
                TerrainSystem.SetTerrain(wall, p, room);
            }
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(fixupMask, clearArray: false);
        }
    }

    /// <summary>
    ///     Greedy maximal-rectangle decomposition over <paramref name="filled" />. Each
    ///     output rectangle is a contiguous block of filled cells; the union of all output
    ///     rectangles equals <paramref name="filled" />. Cells walked in row-major order
    ///     and each seed is grown right then down for deterministic output.
    ///     <para>
    ///         Each synthesized chunk is recorded against
    ///         <see cref="NormalMapFragmentData.Deferred" />.<see cref="MapFragment.SourceName" />
    ///         on the per-level and per-game <see cref="GenerationStats.FragmentInstancesByName" />
    ///         counters via <see cref="RoomLayout.IncrementInstanceCounts" /> — same path
    ///         every other placed fragment uses — so post-mortem diagnostics (see the
    ///         <c>FragmentCompositionDistributionProbe</c>) can correlate the
    ///         <c>$deferred</c> attribution in
    ///         <see cref="LevelGenerationStats.FragmentRoomFailures" />
    ///         against the number of fixup chunks actually emitted.
    ///     </para>
    /// </summary>
    private static List<Chunk> GroupFilledCellsIntoChunks(RoomComponent room, List<Point> filled, RoomParameters roomParameters)
    {
        var chunks = new List<Chunk>();
        if (filled.Count == 0)
        {
            return chunks;
        }

        filled.Sort(static (a, b) => a.Y != b.Y ? a.Y - b.Y : a.X - b.X);
        var remaining = new HashSet<Point>(filled);
        var deferredFragment = room.Level.Game.Services.Data.NormalMapFragments
            .Get(NormalMapFragmentData.Deferred.Name);

        foreach (var seed in filled)
        {
            if (!remaining.Contains(seed))
            {
                continue;
            }

            int x2 = seed.X;
            while (x2 + 1 < room.Width && remaining.Contains(new Point((byte)(x2 + 1), seed.Y)))
            {
                x2++;
            }

            int y2 = seed.Y;
            while (y2 + 1 < room.Height)
            {
                var nextY = y2 + 1;
                var rowFilled = true;
                for (var x = seed.X; x <= x2; x++)
                {
                    if (!remaining.Contains(new Point(x, (byte)nextY)))
                    {
                        rowFilled = false;
                        break;
                    }
                }
                if (!rowFilled)
                {
                    break;
                }
                y2 = nextY;
            }

            var insidePoints = new HashSet<Point>();
            var perimeterPoints = new HashSet<Point>();
            for (var y = seed.Y; y <= y2; y++)
            {
                for (var x = seed.X; x <= x2; x++)
                {
                    var p = new Point(x, y);
                    remaining.Remove(p);
                    var feature = room.Terrain[room.PointToIndex![x, y]];
                    if (feature is MapFeature.StoneWall or MapFeature.RockWall)
                    {
                        perimeterPoints.Add(p);
                    }
                    else
                    {
                        insidePoints.Add(p);
                    }
                }
            }

            chunks.Add(new Chunk(
                room,
                new Rectangle(seed, new Point((byte)x2, (byte)y2)),
                deferredFragment,
                doorwayPoints: null,
                insidePoints: insidePoints,
                perimeterPoints: perimeterPoints,
                roomParameters: roomParameters));
            RoomLayout.IncrementInstanceCounts(room, deferredFragment);
        }

        return chunks;
    }

    /// <summary>
    ///     Populates <paramref name="result" /> with the set of <see cref="MapFeature" />
    ///     values that satisfy every Filled / Doorway / tile-specific condition in
    ///     <paramref name="conditions" /> and returns <c>true</c> when at least one such
    ///     feature exists. Returns <c>false</c> — with <paramref name="result" /> left
    ///     empty — in either of the cases that ask the fixup pass to leave the cell as
    ///     <see cref="MapFeature.Default" />:
    ///     <list type="bullet">
    ///         <item>
    ///             Any <see cref="EdgeConditionKind.Unfilled" /> or
    ///             <see cref="EdgeConditionKind.Edge" /> entry: those conditions require
    ///             the cell to remain empty, so the right answer is to skip the cell.
    ///             The fixup pass treats this the same as a hard contradiction.
    ///         </item>
    ///         <item>
    ///             The positive constraints intersect to the empty set (e.g. two
    ///             disjoint tile-specific demands). The placement-time checks evaluated
    ///             each chunk's conditions in isolation, so the contradiction only
    ///             surfaces here; the fixup pass skips the cell and the defensive
    ///             connectivity sweep walls off any isolated pocket that results.
    ///         </item>
    ///     </list>
    /// </summary>
    private static bool TryCollectSatisfyingFeatures(
        ReadOnlySpan<EdgeConditionKind> conditions, List<MapFeature> result)
    {
        // Unfilled / Edge entries require the cell to stay Default; skip filling.
        for (var i = 0; i < conditions.Length; i++)
        {
            if (conditions[i] is EdgeConditionKind.Unfilled or EdgeConditionKind.Edge)
            {
                return false;
            }
        }

        // Intersect the candidate set across all positive constraints. Default is
        // intentionally excluded — the fixup exists to fill empty cells with something
        // non-empty.
        AppendAllPlaceableFeatures(result);

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            switch (condition)
            {
                case EdgeConditionKind.Default:
                case EdgeConditionKind.Filled:
                    // Default poses no constraint; Filled is satisfied by every feature
                    // already in the candidate list (all non-Default).
                    break;
                case EdgeConditionKind.Doorway:
                    IntersectWith(result, MapFeature.StoneFloor, MapFeature.StoneArchway);
                    break;
                default:
                    // Tile-specific: numeric value matches MapFeature.
                    IntersectWith(result, (MapFeature)condition);
                    break;
            }

            if (result.Count == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static void AppendAllPlaceableFeatures(List<MapFeature> result)
    {
        foreach (var feature in Enum.GetValues<MapFeature>())
        {
            if (feature is not (MapFeature.Default
                or MapFeature.InactiveDeityAltar
                or MapFeature.InactiveMajorShrine
                or MapFeature.InactiveMinorShrine
                or MapFeature.InactiveRitualShrine
                or MapFeature.InactiveSmallFountain
                or MapFeature.InactiveBigFountain
                or MapFeature.InactiveSkillPedestal
                or MapFeature.InactiveRacePedestal
                or MapFeature.InactiveTraitPedestal
                or MapFeature.InactiveFeatPedestal
                or MapFeature.InactiveArmorStand
                or MapFeature.InactiveGildedArmorStand
                or MapFeature.InactiveItemCase
                or MapFeature.InactiveGildedItemCase
                or MapFeature.InactiveWeaponStand
                or MapFeature.InactiveGildedWeaponStand
                or MapFeature.Unexplored))
            {
                result.Add(feature);
            }
        }
    }

    private static void IntersectWith(List<MapFeature> result, MapFeature only)
    {
        for (var i = result.Count - 1; i >= 0; i--)
        {
            if (result[i] != only)
            {
                result.RemoveAt(i);
            }
        }
    }

    private static void IntersectWith(List<MapFeature> result, MapFeature a, MapFeature b)
    {
        for (var i = result.Count - 1; i >= 0; i--)
        {
            var f = result[i];
            if (f != a && f != b)
            {
                result.RemoveAt(i);
            }
        }
    }

    private static MapFeature PickPreferredFixupFeature(
        List<MapFeature> candidates, Point point, RoomComponent room, SimpleRandom random, MapFeature defaultWall)
    {
        // Prefer an existing adjacent wall's material when the conditions allow a wall, so the
        // synthesized fill extends the surrounding wall line instead of introducing a foreign
        // wall material; fall back to the branch's default wall, then any wall.
        var adjacentWall = RoomLayout.PickAdjacentWall(room, point, defaultWall);
        if (candidates.Contains(adjacentWall))
        {
            return adjacentWall;
        }
        if (candidates.Contains(defaultWall))
        {
            return defaultWall;
        }
        if (candidates.Contains(MapFeature.StoneWall))
        {
            return MapFeature.StoneWall;
        }
        if (candidates.Contains(MapFeature.RockWall))
        {
            return MapFeature.RockWall;
        }

        // Try a feature already carried by a cardinal neighbor so the fixup cell
        // visually extends the adjacent terrain instead of introducing an isolated tile.
        var pointToIndex = room.PointToIndex!;
        var width = room.Width;
        var height = room.Height;
        if (point.Y > 0)
        {
            var north = room.Terrain[pointToIndex[point.X, point.Y - 1]];
            if (north != MapFeature.Default && candidates.Contains(north))
            {
                return north;
            }
        }
        if (point.Y + 1 < height)
        {
            var south = room.Terrain[pointToIndex[point.X, point.Y + 1]];
            if (south != MapFeature.Default && candidates.Contains(south))
            {
                return south;
            }
        }
        if (point.X > 0)
        {
            var west = room.Terrain[pointToIndex[point.X - 1, point.Y]];
            if (west != MapFeature.Default && candidates.Contains(west))
            {
                return west;
            }
        }
        if (point.X + 1 < width)
        {
            var east = room.Terrain[pointToIndex[point.X + 1, point.Y]];
            if (east != MapFeature.Default && candidates.Contains(east))
            {
                return east;
            }
        }

        return candidates[random.Next(0, candidates.Count)];
    }

    public Chunk? TryPlace(RoomComponent room, in LotPlacementContext ctx)
    {
        try
        {
            EnsureInitialized();

            var chunk = ByteMap.Length != 0
                ? TryPlaceStaticMap(room, ctx, ctx.MinDoorways)
                : TryPlace(room, ctx.Lot, DynamicMap, ctx, ctx.MinDoorways, ctx.DemandedDoorways, ctx.TargetEdgeFlag);

            return chunk;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error while placing fragment " + Name, ex);
        }
    }

    /// <summary>
    ///     Resolves wildcard <see cref="FeatureWildcardGlyph" /> entries in the freshly
    ///     parsed <paramref name="featurePoints" /> list by binding each one to a
    ///     <see cref="RoomFeatures" /> bit that <paramref name="room" /> still demands but
    ///     no chunk has placed yet. Concrete feature bits already in the list are folded
    ///     into <see cref="RoomComponent.BoundPlaceableFeatures" /> first, so a wildcard
    ///     sharing the chunk with a concrete glyph for the same feature never double-binds.
    ///     Wildcards beyond the room's outstanding count are left unbound (the
    ///     materialization pass skips them).
    /// </summary>
    /// <remarks>
    ///     <see cref="RoomComponent.BoundPlaceableFeatures" /> accumulates across every
    ///     <see cref="TryPlace(RoomComponent, in LotPlacementContext)" /> call for the same
    ///     room, so wildcards in later chunks see the bits already consumed by earlier
    ///     chunks. Layouts must place fragments with concrete feature glyphs before
    ///     fragments that rely on wildcards when a specific binding is required — earlier
    ///     wildcards may otherwise grab a bit that a later chunk would have placed
    ///     concretely. Mutates <paramref name="featurePoints" /> in place before the
    ///     <see cref="Chunk" /> is constructed so the chunk's exposed list is immutable.
    /// </remarks>
    private static void BindWildcardFeatures(RoomComponent room, List<(Point Point, RoomFeatures? Feature)> featurePoints)
    {
        var demanded = room.Features & PlaceableFeaturesMask;
        if (demanded == 0)
        {
            return;
        }

        // Fold this chunk's concrete bits into the room's cumulative bound set before
        // picking wildcards, so a wildcard in the same chunk can't reuse a bit the
        // concrete glyph already satisfied. Trapped is excluded so its demand is never
        // marked satisfied — traps overlay floor and don't compete with a singular
        // feature slot the way a fountain or altar does, so multiple trap fragments
        // remain individually demanded.
        var wildcardCount = 0;
        for (var i = 0; i < featurePoints.Count; i++)
        {
            var feature = featurePoints[i].Feature;
            if (feature.HasValue)
            {
                if (feature.Value != RoomFeatures.Trapped)
                {
                    room.BoundPlaceableFeatures |= feature.Value;
                }
            }
            else
            {
                wildcardCount++;
            }
        }

        if (wildcardCount == 0)
        {
            return;
        }

        var outstanding = demanded & ~room.BoundPlaceableFeatures;
        if (outstanding == 0)
        {
            return;
        }

        var outstandingBits = new List<RoomFeatures>();
        foreach (var bit in PlaceableFeatureBits)
        {
            if ((outstanding & bit) != 0)
            {
                outstandingBits.Add(bit);
            }
        }

        var random = room.Level.GenerationRandom;
        for (var i = 0; i < featurePoints.Count; i++)
        {
            if (featurePoints[i].Feature.HasValue)
            {
                continue;
            }

            if (outstandingBits.Count == 0)
            {
                return;
            }

            var pickIndex = random.Next(0, outstandingBits.Count);
            var picked = outstandingBits[pickIndex];
            outstandingBits.RemoveAt(pickIndex);
            featurePoints[i] = (featurePoints[i].Point, picked);
            room.BoundPlaceableFeatures |= picked;

            // The wildcard glyph initially committed plain stone floor via Write; now that
            // the bit is bound, overwrite the terrain with the feature-specific MapFeature
            // so downstream sensing / rendering reflect the resolved feature.
            if (RoomFeatureToMapFeature.TryGetValue(picked, out var mappedFeature))
            {
                TerrainSystem.SetTerrain(mappedFeature, featurePoints[i].Point, room);
            }
        }
    }

    protected virtual Chunk? TryPlaceStaticMap(RoomComponent room, in LotPlacementContext ctx, int minDoorways)
    {
        Point target;
        // Reuse the anchor the weighting pass already computed for this fragment in this lot
        // (see GetEdgeMatchMultiplier) when available, instead of sweeping for it again.
        if (ctx.AnchorCache is { } anchorCache && anchorCache.TryGet(this, out target))
        {
            // Cached anchor reused as-is.
        }
        else if (!TryComputeAnchor(room, ctx, room.Level.GenerationRandom, out target, out _))
        {
            // Fall back to the unconstrained random placement so callers that previously
            // relied on the no-context path still work (e.g. defining-fragment placement).
            // A null fallback means the payload simply doesn't fit the lot (size mismatch
            // or every candidate position fails the constraint pass); the placement loop
            // in RoomLayout.TryPlace skips fragments that return null and continues with
            // the next weighted candidate, so this is not an error.
            var fallback = ctx.Lot.PlaceInside(PayloadArea, room.Level.GenerationRandom);
            if (!fallback.HasValue)
            {
                return null;
            }
            target = fallback.Value;
        }

        var doorwayPoints = new HashSet<Point>();
        var perimeterPoints = new HashSet<Point>();
        var insidePoints = new HashSet<Point>();
        var points = new List<Point>();
        var featurePoints = new List<(Point Point, RoomFeatures? Feature)>();
        var creaturePoints = new List<Point>();
        var trapPoints = new List<Point>();
        WriteMap(target, room, Write,
            (doorwayPoints, perimeterPoints, insidePoints, points, featurePoints, creaturePoints, trapPoints, ctx));

        // Resolve wildcard feature glyphs against the room's outstanding placeable features
        // before constructing the chunk so the chunk's FeaturePoints list is fully bound.
        BindWildcardFeatures(room, featurePoints);

        var chunkBounds = new Rectangle(target, PayloadArea.Width, PayloadArea.Height);
        if (!NoRandomDoorways && doorwayPoints.Count == 0)
        {
            FindStaticMapDoorwayPoints(chunkBounds, room, doorwayPoints);
            if (doorwayPoints.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Static fragment '{Name}' placed at {chunkBounds} has no doorway candidates. "
                    + "Add '+' markers, set NoRandomDoorways=true, or surround the fragment with reachable terrain.");
            }
        }

        // The weight pass should already have filtered out static
        // fragments that can't meet minDoorways, so reaching here with too few doorways
        // is a contract violation — usually a random-fallback fragment whose neighbor
        // geometry yielded fewer wall-adjacent candidates than the lot demands.
        // Open-perimeter fragments (e.g. corner pieces with Filled conditional edges)
        // intentionally carry no `+` markers: their conditional-edge cells are flush
        // with adjacent placed terrain and serve as implicit doorways. The room-layout
        // demand is met by the placed-chunk on the other side of the open perimeter,
        // not by walk-through tiles inside this chunk, so the count check doesn't apply.
        if (doorwayPoints.Count < minDoorways && !HasOpenPerimeter)
        {
            throw new InvalidOperationException(
                $"Static fragment '{Name}' placed at {chunkBounds} produced {doorwayPoints.Count} "
                + $"doorway(s) but the lot requires at least {minDoorways}. "
                + "Author more '+' markers or tighten the fragment's weight expression.");
        }

        return new Chunk(room,
            chunkBounds,
            this,
            doorwayPoints,
            insidePoints,
            perimeterPoints,
            ctx.RoomParameters,
            featurePoints,
            creaturePoints,
            trapPoints);
    }

    protected void Write(
        char c,
        Point point,
        RoomComponent room,
        (HashSet<Point> doorwayPoints,
            HashSet<Point> perimeterPoints,
            HashSet<Point> insidePoints,
            List<Point> points,
            List<(Point Point, RoomFeatures? Feature)> featurePoints,
            List<Point> creaturePoints,
            List<Point> trapPoints,
            LotPlacementContext ctx) state)
    {
        var (doorwayPoints, perimeterPoints, insidePoints, points,
            featurePoints, creaturePoints, trapPoints, ctx) = state;

        MapFeature feature;
        switch (c)
        {
            // ' ' is a no-op (typically padding on a short row); every other glyph
            // contributes both a tile and a chunk-bookkeeping entry.
            case ' ':
                return;
            case '.':
                insidePoints.Add(point);
                feature = MapFeature.StoneFloor;
                break;
            case ',':
                insidePoints.Add(point);
                feature = MapFeature.RockFloor;
                break;
            case '+':
                doorwayPoints.Add(point);
                perimeterPoints.Add(point);
                feature = MapFeature.StoneFloor;
                break;
            case '#':
                perimeterPoints.Add(point);
                feature = MapFeature.StoneWall;
                break;
            case 'A':
                perimeterPoints.Add(point);
                feature = MapFeature.StoneArchway;
                break;
            case '~':
                feature = MapFeature.Pool;
                break;
            case '<':
            case '>':
                CreateConnection(room, point, c);
                feature = MapFeature.StoneFloor;
                break;
            case '^':
                // Trap materialization happens in a follow-up pass; the position is recorded
                // on the chunk.
                insidePoints.Add(point);
                trapPoints.Add(point);
                featurePoints.Add((point, RoomFeatures.Trapped));
                feature = MapFeature.StoneFloor;
                break;
            case CreatureWildcardGlyph:
                insidePoints.Add(point);
                creaturePoints.Add(point);
                feature = MapFeature.StoneFloor;
                break;
            default:
                insidePoints.Add(point);
                if (FeatureGlyphs.TryGetValue(c, out var roomFeature))
                {
                    featurePoints.Add((point, roomFeature));
                    // Wildcard glyph ($) defers feature binding to BindWildcardFeatures
                    // and writes plain floor in the meantime; concrete feature glyphs
                    // commit their feature-specific MapFeature directly.
                    feature = roomFeature.HasValue
                        ? RoomFeatureToMapFeature[roomFeature.Value]
                        : MapFeature.StoneFloor;
                }
                else
                {
                    feature = MapFeature.StoneFloor;
                }

                break;
        }

        points.Add(point);

        if (IsWallFeature(feature) && ShouldPreserveAgainstWallWrite(room, point, ctx))
        {
            // A previously placed chunk owns this cell and its terrain is walkable
            // (floor / archway / pool) or otherwise important. Skip the wall write so
            // a neighbor can't seal off the chunk's open perimeter, doorway, or
            // themed feature. The cell is still recorded as part of THIS chunk's
            // outline (via points / perimeterPoints above) so corridor logic treats it
            // as a shared wall on this chunk's side.
            return;
        }

        TerrainSystem.SetTerrain(feature, point, room);
    }

    internal static bool IsWallFeature(MapFeature feature)
        => feature is MapFeature.StoneWall or MapFeature.RockWall;

    /// <summary>
    ///     Returns <c>true</c> when an incoming wall write at <paramref name="point" />
    ///     should be skipped because a previously-placed chunk owns the cell and that
    ///     chunk's content must not be clobbered.
    ///     <para>
    ///         Invariant maintained by the placement pipeline: <em>only the room's
    ///         default terrain is allowed outside of a chunk</em>. Every cell that
    ///         differs from <see cref="LotPlacementContext.DefaultTerrain" /> belongs
    ///         to some chunk recorded in <see cref="LotPlacementContext.ChunkCells" />
    ///         (the defining fragment is the first chunk and claims its cells like
    ///         any other). Cells <em>at</em> the default value are background fill
    ///         and freely overwritable.
    ///     </para>
    ///     <para>
    ///         Connection cells (stairs / portals) are always preserved.
    ///     </para>
    ///     <para>
    ///         The dynamic-map path registers the chunk's perimeter cells with
    ///         <see cref="LotPlacementContext.RoomParameters" /> during
    ///         <see cref="Chunk" /> construction (which happens inside
    ///         <see cref="BuildChunk" /> BEFORE the caller's wall writes). Without
    ///         <paramref name="currentChunk" /> the predicate would see the chunk
    ///         owning its own perimeter cells and incorrectly skip them, leaving the
    ///         room with default-terrain gaps where walls should be. Callers in the
    ///         dynamic path pass the chunk they're writing so its own perimeter
    ///         registration doesn't suppress the wall write.
    ///     </para>
    /// </summary>
    private static bool ShouldPreserveAgainstWallWrite(
        RoomComponent room, Point point, in LotPlacementContext ctx, Chunk? currentChunk = null)
    {
        if (room.Connections.ContainsKey(point))
        {
            return true;
        }

        // RoomParameters knows exactly which cells belong to a chunk. Cells whose
        // entry is null are background default terrain, freely overwritable by the
        // new chunk's wall write. Cells owned by `currentChunk` are this chunk's
        // own outline and must not be skipped — they are precisely what we're
        // writing now.
        var owner = ctx.GetChunkAt(point);
        return owner is not null && !ReferenceEquals(owner, currentChunk);
    }

    /// <summary>
    ///     A cell is "important" when it already carries a non-default, non-wall, non-floor
    ///     terrain feature (a pool, archway, etc.) or a connection placed by a previously
    ///     written chunk. Used by <see cref="Write" /> and the dynamic-map perimeter action
    ///     so a candidate's wall write doesn't overwrite content contributed by a
    ///     dense-layout neighbor whose perimeter the candidate overlaps. Plain
    ///     <see cref="MapFeature.StoneFloor" />/<see cref="MapFeature.RockFloor" /> tiles
    ///     are not treated as important — they're the default tiling and may legitimately
    ///     be walled over by later chunks.
    /// </summary>
    protected static bool IsImportantCell(RoomComponent room, Point point)
    {
        if (room.Connections.ContainsKey(point))
        {
            return true;
        }

        var current = room.Terrain[room.PointToIndex![point.X, point.Y]];
        return current is not (MapFeature.Default
            or MapFeature.StoneWall or MapFeature.RockWall
            or MapFeature.StoneFloor or MapFeature.RockFloor);
    }

    protected virtual void CreateConnection(RoomComponent room, Point point, char? glyph)
    {
    }

    protected Chunk? TryPlace(RoomComponent room, Rectangle boundingRectangle, DynamicMap? map,
        in LotPlacementContext ctx,
        int minDoorways = 0, DirectionFlags preferredEdges = DirectionFlags.None,
        DirectionFlags flushEdges = DirectionFlags.None)
    {
        if (map == null || !boundingRectangle.Contains(map.MinSize))
        {
            return null;
        }

        // BuildChunk only computes the chunk's shape (inside / perimeter / doorway
        // points) and constructs the Chunk — we write terrain here so the wall-vs-
        // walkable preservation rule can be evaluated against the current ctx without
        // capturing it into a lambda closure.
        var chunk = BuildChunk(
            room,
            map.GetRoomPoints(room, boundingRectangle, preferredEdges, flushEdges),
            minDoorways,
            ctx.RoomParameters);
        if (chunk is null)
        {
            return null;
        }

        foreach (var p in chunk.InsidePoints)
        {
            TerrainSystem.SetTerrain(MapFeature.StoneFloor, p, room);
        }

        foreach (var p in chunk.PerimeterPoints)
        {
            if (ShouldPreserveAgainstWallWrite(room, p, ctx, chunk))
            {
                // A previously-placed chunk owns this cell with walkable content;
                // preserve it instead of sealing it with a wall.
                continue;
            }
            TerrainSystem.SetTerrain(MapFeature.StoneWall, p, room);
        }

        return chunk;
    }

    public void WriteMap<TState>(Point target, RoomComponent room,
        Action<char, Point, RoomComponent, TState> write, TState state)
    {
        EnsureInitialized();

        var map = ByteMap;
        var annotations = Annotations;
        var random = annotations is { Count: > 0 } ? room.Level.GenerationRandom : null;
        // target is the room-space top-left of the fragment's payload (matches
        // chunkBounds in TryPlaceStaticMap). Translate each iterated payload point into
        // room space directly via (target + (fragmentPoint - payloadOrigin)).
        var payloadOrigin = PayloadArea.TopLeft;
        foreach (var fragmentPoint in PayloadArea)
        {
            var mapPoint = (char)map[PointToIndex[fragmentPoint.X, fragmentPoint.Y]];
            if (random is not null && annotations!.TryGetValue(mapPoint, out var substitutes))
            {
                mapPoint = substitutes[random.Next(0, substitutes.Length)];
            }

            var writePoint = new Point(
                (byte)(target.X + fragmentPoint.X - payloadOrigin.X),
                (byte)(target.Y + fragmentPoint.Y - payloadOrigin.Y));
            write(mapPoint, writePoint, room, state);
        }
    }

    /// <summary>
    ///     Computes a chunk's shape (inside, perimeter and doorway points) from
    ///     <paramref name="points" /> and constructs a <see cref="Chunk" /> when the
    ///     resulting doorway count meets <paramref name="minDoorways" />. The method
    ///     intentionally does NOT write terrain — the caller is responsible for
    ///     iterating <see cref="Chunk.InsidePoints" /> / <see cref="Chunk.PerimeterPoints" />
    ///     and applying its own write policy (e.g. <see cref="ShouldPreserveAgainstWallWrite" />
    ///     for walls). This keeps the method allocation-free of caller-state closures.
    /// </summary>
    public virtual Chunk? BuildChunk(RoomComponent room, IEnumerable<Point> points,
        int minDoorways, RoomParameters roomParameters)
    {
        var insidePoints = new HashSet<Point>();
        var neighbors = new byte[room.TileCount];
        var firstPoint = new Point();
        var lastPoint = new Point();
        var left = byte.MaxValue;
        var right = byte.MinValue;
        var occupiedPointCount = 0;
        foreach (var chunkPoint in points)
        {
            if (occupiedPointCount == 0)
            {
                firstPoint = chunkPoint;
                right = firstPoint.X;
                left = firstPoint.X;
            }
            else if (lastPoint.Y != chunkPoint.Y)
            {
                right = Math.Max(right, lastPoint.X);
                left = Math.Min(left, chunkPoint.X);
            }

            lastPoint = chunkPoint;

            for (var directionIndex = 0; directionIndex < 8; directionIndex++)
            {
                var direction = Vector.MovementDirections[directionIndex];
                var newLocationX = (byte)(chunkPoint.X + direction.X);
                var newLocationY = (byte)(chunkPoint.Y + direction.Y);

                if (newLocationX >= room.Width || newLocationY >= room.Height)
                {
                    continue;
                }

                var newLocationIndex = room.PointToIndex![newLocationX, newLocationY];
                var newNeighbors = (byte)(neighbors[newLocationIndex] |
                                           1 << Vector.OppositeDirectionIndexes[directionIndex]);
                if (newNeighbors == (byte)DirectionFlags.All &&
                    (neighbors[room.PointToIndex[chunkPoint.X, chunkPoint.Y]] & 1 << directionIndex) != 0)
                {
                    insidePoints.Add(new Point(newLocationX, newLocationY));
                }

                neighbors[newLocationIndex] = newNeighbors;
            }

            occupiedPointCount++;
        }

        if (occupiedPointCount == 0)
        {
            return null;
        }

        // Compute perimeter and doorway points from the neighbor map. No terrain is
        // touched here; the caller writes terrain after we return the shape.
        var perimeterPoints = new HashSet<Point>();
        var perimeter = WalkPerimeter(firstPoint, neighbors, room.PointToIndex!, perimeterPoints);
        var doorwayPoints = FindDoorwayPoints(perimeter, room);

        if (doorwayPoints.Count < minDoorways)
        {
            return null;
        }

        return new Chunk(room,
            new Rectangle(new Point(left, firstPoint.Y), new Point(right, lastPoint.Y)),
            this,
            doorwayPoints,
            insidePoints,
            perimeterPoints,
            roomParameters);
    }

    /// <summary>
    ///     Scans the perimeter of <paramref name="chunkBounds" /> for wall tiles whose inward
    ///     orthogonal neighbor is floor and whose outward neighbor is corridor-able
    ///     (default-fill or floor). Such tiles can host a door connecting the chunk to the
    ///     surrounding empty space.
    /// </summary>
    private static void FindStaticMapDoorwayPoints(Rectangle chunkBounds, RoomComponent room, HashSet<Point> doorwayPoints)
    {
        foreach (var (edge, inside, outside) in EnumeratePerimeterEdgeCells(chunkBounds))
        {
            if (!room.IsValid(edge) || !room.IsValid(inside) || !room.IsValid(outside))
            {
                continue;
            }

            if (room.Terrain[room.PointToIndex![edge.X, edge.Y]] != MapFeature.StoneWall)
            {
                continue;
            }

            var insideFeature = room.Terrain[room.PointToIndex[inside.X, inside.Y]];
            if (insideFeature != MapFeature.StoneFloor && insideFeature != MapFeature.RockFloor)
            {
                continue;
            }

            if (!RoomLayout.CanPlaceCorridor(outside, room))
            {
                continue;
            }

            doorwayPoints.Add(edge);
        }
    }

    /// <summary>
    ///     Enumerates each non-corner cell on <paramref name="bounds" />'s perimeter together
    ///     with its inward and outward orthogonal neighbors. Corner cells (where two axes
    ///     coincide with the boundary) are skipped since their inward direction is ambiguous.
    /// </summary>
    private static IEnumerable<(Point Edge, Point Inside, Point Outside)> EnumeratePerimeterEdgeCells(Rectangle bounds)
    {
        var topLeft = bounds.TopLeft;
        var bottomRight = bounds.BottomRight;
        foreach (var point in bounds.GetPerimeter())
        {
            var onLeft = point.X == topLeft.X;
            var onRight = point.X == bottomRight.X;
            var onTop = point.Y == topLeft.Y;
            var onBottom = point.Y == bottomRight.Y;
            if ((onLeft || onRight) && (onTop || onBottom))
            {
                continue;
            }

            int inDx, inDy;
            if (onLeft)
            {
                inDx = 1;
                inDy = 0;
            }
            else if (onRight)
            {
                inDx = -1;
                inDy = 0;
            }
            else if (onTop)
            {
                inDx = 0;
                inDy = 1;
            }
            else
            {
                inDx = 0;
                inDy = -1;
            }

            var inside = new Point((byte)(point.X + inDx), (byte)(point.Y + inDy));
            var outside = new Point((byte)(point.X - inDx), (byte)(point.Y - inDy));
            yield return (point, inside, outside);
        }
    }

    private static HashSet<Point> FindDoorwayPoints(
        IReadOnlyList<Point> perimeter, RoomComponent room, bool allowCorners = false)
    {
        var doorwayPoints = new HashSet<Point>();
        for (var i = 0; i < perimeter.Count; i++)
        {
            var point = perimeter[i];
            var nextPoint = perimeter[i == perimeter.Count - 1 ? 0 : i + 1];
            if (point.DistanceTo(nextPoint) > 1)
            {
                i++;
                continue;
            }

            var previousPoint = perimeter[i == 0 ? perimeter.Count - 1 : i - 1];
            if (point.DistanceTo(previousPoint) > 1)
            {
                continue;
            }

            var direction = previousPoint.DifferenceTo(nextPoint).GetUnit();
            if (!allowCorners && direction.X != 0 && direction.Y != 0)
            {
                continue;
            }

            var firstOrthogonal = direction.GetOrthogonal();
            var secondOrthogonal = firstOrthogonal.GetInverse();

            var corridorCandidatePoint = point.Translate(firstOrthogonal);
            if (!room.IsValid(corridorCandidatePoint) || !RoomLayout.CanPlaceCorridor(corridorCandidatePoint, room))
            {
                continue;
            }

            var doorwayCandidatePoint = point.Translate(secondOrthogonal);
            if (!room.IsValid(doorwayCandidatePoint) || !RoomLayout.CanPlaceCorridor(doorwayCandidatePoint, room))
            {
                continue;
            }

            doorwayPoints.Add(point);
        }

        return doorwayPoints;
    }

    private static IReadOnlyList<Point> WalkPerimeter(Point firstPoint, byte[] neighbors, short[,] pointToIndex,
        HashSet<Point> perimeterPoints) =>
        WalkPerimeter(new Point(firstPoint.X, (byte)(firstPoint.Y - 1)), DirectionFlags.South, Direction.North,
            new Dictionary<Point, int>(), neighbors, pointToIndex, perimeterPoints) ??
        new List<Point>();

    private static List<Point>? WalkPerimeter(Point firstPoint, DirectionFlags currentNeighborMap,
        Direction previousPointDirection, Dictionary<Point, int> visitedIntersections, byte[] neighbors,
        short[,] pointToIndex, HashSet<Point> perimeterPoints)
    {
        // Assuming both the perimeter and the inner area are contiguous
        List<Point>? perimeter = null;
        var currentPoint = firstPoint;
        var previousPointDirectionIndex = (byte)previousPointDirection;
        var perimeterLooped = false;

        for (var j = 0; j < neighbors.Length + 2; j++)
        {
            var perimeterPointFound = false;
            for (var i = 3; i > 0; i--)
            {
                var directionIndexToCheck = (byte)((previousPointDirectionIndex + i * 2) % 8);
                var neighborDirection = (DirectionFlags)(1 << directionIndexToCheck);
                if ((currentNeighborMap & neighborDirection) == neighborDirection)
                {
                    var directionToCheck = Vector.MovementDirections[directionIndexToCheck];
                    var newLocationX = (byte)(currentPoint.X + directionToCheck.X);
                    var newLocationY = (byte)(currentPoint.Y + directionToCheck.Y);
                    var nextPoint = new Point(newLocationX, newLocationY);
                    if (nextPoint.Equals(firstPoint))
                    {
                        break;
                    }

                    var nextNeighborMap = (DirectionFlags)neighbors[pointToIndex[newLocationX, newLocationY]];
                    var nextIsPerimeter = IsPerimeter(nextNeighborMap);
                    if (nextIsPerimeter)
                    {
                        if (perimeterLooped)
                        {
                            break;
                        }

                        perimeter ??= new List<Point>();

                        if (perimeter.Count == 0 || !nextPoint.Equals(perimeter[0]))
                        {
                            perimeter.Add(nextPoint);
                            perimeterPoints.Add(nextPoint);
                        }
                        else
                        {
                            perimeterLooped = true;
                        }

                        currentPoint = nextPoint;
                        currentNeighborMap = nextNeighborMap;
                        previousPointDirectionIndex = Vector.OppositeDirectionIndexes[directionIndexToCheck];
                        perimeterPointFound = true;
                        break;
                    }

                    if (visitedIntersections.TryGetValue(nextPoint, out var _))
                    {
                        continue;
                    }

                    visitedIntersections[nextPoint] = directionIndexToCheck;

                    var subPerimeter = WalkPerimeter(nextPoint, nextNeighborMap,
                        (Direction)Vector.OppositeDirectionIndexes[directionIndexToCheck], visitedIntersections,
                        neighbors, pointToIndex, perimeterPoints);

                    if (subPerimeter != null)
                    {
                        if (perimeter != null)
                        {
                            throw new InvalidOperationException("Unconnected perimeters");
                        }

                        perimeter = subPerimeter;
                    }
                }
            }

            if (!perimeterPointFound)
            {
                return perimeter;
            }
        }

        throw new InvalidOperationException("Infinite loop finding the room perimeter");
    }

    private static bool IsPerimeter(DirectionFlags neighborMap) =>
        (neighborMap & DirectionFlags.NorthwestCorner) == DirectionFlags.NorthwestCorner
        || (neighborMap & DirectionFlags.NortheastCorner) == DirectionFlags.NortheastCorner
        || (neighborMap & DirectionFlags.SoutheastCorner) == DirectionFlags.SoutheastCorner
        || (neighborMap & DirectionFlags.SouthwestCorner) == DirectionFlags.SouthwestCorner;

    protected static Dictionary<string, Func<TMapFragment, object?, bool>> GetPropertyConditions<TMapFragment>()
        where TMapFragment : MapFragment => new()
    {
        { nameof(Name), (_, v) => v != null },
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        { nameof(GenerationWeight), (_, v) => v != null && (string)v != DefaultWeight },
        { nameof(NoMirror), (_, v) => (bool)v! },
        { nameof(NoTurnOver), (_, v) => (bool)v! },
        { nameof(NoTransform), (_, v) => (bool)v! },
        { nameof(NoOverwrite), (_, v) => (bool)v! },
        { nameof(Map), (_, v) => !string.IsNullOrEmpty((string)v!) }
    };

    public abstract ICSScriptSerializer GetSerializer();
}
