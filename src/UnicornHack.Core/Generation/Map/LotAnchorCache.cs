namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-lot scratch cache that lets a fragment's placement anchor — computed by
///     <see cref="MapFragment.GetEdgeMatchMultiplier" /> during the weighted-selection pass —
///     be reused by <see cref="MapFragment.TryPlaceStaticMap" /> when that fragment wins the
///     pick, instead of recomputing the (geometric) anchor sweep a second time. The cache is
///     keyed by fragment instance and lives for a single lot: the owning room layout
///     <see cref="UniformRoomLayout.PlaceSurroundingFragments" /> clears it before each lot so
///     entries never leak across lots (whose terrain / demanded doorways differ).
/// </summary>
public sealed class LotAnchorCache
{
    private readonly Dictionary<MapFragment, Point> _anchors = new();

    public void Clear() => _anchors.Clear();

    public void Store(MapFragment fragment, Point anchor) => _anchors[fragment] = anchor;

    public bool TryGet(MapFragment fragment, out Point anchor) => _anchors.TryGetValue(fragment, out anchor);
}
