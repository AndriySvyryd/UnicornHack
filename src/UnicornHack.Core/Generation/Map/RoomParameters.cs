using System.Buffers;
using System.Runtime.CompilerServices;

namespace UnicornHack.Generation.Map;

/// <summary>
///     Per-room snapshot of the effective values produced by combining a
///     <see cref="DefiningMapFragment" /> with the level's <see cref="Branch" /> defaults.
///     Returned by <see cref="DefiningMapFragment.GetRoomParameters" /> so callers don't have
///     to invoke five separate <c>GetEffective*</c> methods.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="DefaultTerrain" /> records the background fill applied by
///         <see cref="LevelLayout.AllocateTerrain" />. Placement-time checks
///         distinguish "background / unwritten" cells from chunk-written ones by
///         comparing terrain against this value, so branches whose default isn't
///         <see cref="MapFeature.Default" /> (e.g. cavern branches pre-filled with
///         <see cref="MapFeature.RockFloor" />) are handled correctly.
///     </para>
///     <para>
///         The chunk-cell ownership map (accessed via the <see cref="this[Point]" />
///         indexer) records, for each room cell, which authored chunk has written to
///         it (or <c>null</c> for cells still carrying the room's
///         <see cref="DefaultTerrain" /> background fill). The walkable-wins overlap
///         rule consults this map so a neighbor's wall write can't seal an adjacent
///         chunk's authored floor or doorway. The defining fragment intentionally
///         leaves the map empty for its own cells: it merely establishes the room's
///         background terrain, which is already captured by <see cref="DefaultTerrain" />.
///     </para>
/// </remarks>
public sealed record RoomParameters : IDisposable
{
    private Chunk?[]? _chunkCells;

    public RoomParameters(
        byte RoomWidth,
        byte RoomHeight,
        MapFeature DefaultTerrain,
        MapFeature DefaultPathTerrain,
        MapFeature DefaultWall,
        CreatureGenerator CreatureGenerator,
        ItemGenerator ItemGenerator)
    {
        if (RoomWidth == 0 || RoomHeight == 0)
        {
            throw new ArgumentException(
                $"RoomParameters requires positive width/height; got {RoomWidth}x{RoomHeight}.");
        }
        ArgumentNullException.ThrowIfNull(CreatureGenerator);
        ArgumentNullException.ThrowIfNull(ItemGenerator);

        this.RoomWidth = RoomWidth;
        this.RoomHeight = RoomHeight;
        this.DefaultTerrain = DefaultTerrain;
        this.DefaultPathTerrain = DefaultPathTerrain;
        this.DefaultWall = DefaultWall;
        this.CreatureGenerator = CreatureGenerator;
        this.ItemGenerator = ItemGenerator;

        var cellCount = RoomWidth * RoomHeight;
        var buffer = ArrayPool<Chunk?>.Shared.Rent(cellCount);
        // The pool may hand back a buffer larger than requested, and the slack
        // beyond `cellCount` may carry stale references from a previous tenant.
        // The indexer only ever touches the [0, cellCount) prefix, so clearing
        // just that range is enough.
        Array.Clear(buffer, 0, cellCount);
        _chunkCells = buffer;
    }

    public byte RoomWidth { get; init; }
    public byte RoomHeight { get; init; }
    public MapFeature DefaultTerrain { get; init; }

    /// <summary>
    ///     Effective <see cref="MapFeature" /> used for carved corridor / path tiles (from
    ///     <see cref="DefiningMapFragment.GetEffectiveDefaultPathTerrain" />, falling back to
    ///     <see cref="Branch.DefaultPathTerrain" />). Defaults to <see cref="MapFeature.RockFloor" />.
    /// </summary>
    public MapFeature DefaultPathTerrain { get; init; }

    /// <summary>
    ///     Effective <see cref="MapFeature" /> used for walls the generator places to enclose
    ///     carved corridors and the room background (from
    ///     <see cref="DefiningMapFragment.GetEffectiveDefaultWall" />, falling back to
    ///     <see cref="Branch.DefaultWall" />). Defaults to <see cref="MapFeature.RockWall" />.
    /// </summary>
    public MapFeature DefaultWall { get; init; }

    public CreatureGenerator CreatureGenerator { get; init; }
    public ItemGenerator ItemGenerator { get; init; }

    /// <summary>
    ///     Per-cell chunk ownership entry at <c>(x, y)</c>. A non-null value is the
    ///     chunk that authored the cell; <c>null</c> means the cell still carries
    ///     <see cref="DefaultTerrain" />. Indexing outside <c>[0, RoomWidth) ×
    ///     [0, RoomHeight)</c> is invalid.
    /// </summary>
    public Chunk? this[int x, int y]
    {
        get => _chunkCells![(y * RoomWidth) + x];
        set => _chunkCells![(y * RoomWidth) + x] = value;
    }

    /// <summary>
    ///     <see cref="Point" />-keyed sugar for <see cref="this[int, int]" />. Returns
    ///     the chunk that authored the cell at <paramref name="point" /> or <c>null</c>
    ///     when the cell is background fill.
    /// </summary>
    public Chunk? this[Point point]
    {
        get => _chunkCells![(point.Y * RoomWidth) + point.X];
        set => _chunkCells![(point.Y * RoomWidth) + point.X] = value;
    }

    /// <summary>
    ///     Zeroes every logical chunk-cell entry. Used by tests that rerun placement
    ///     against the same <see cref="RoomParameters" /> instance; the production
    ///     pipeline always uses a fresh one per room.
    /// </summary>
    public void ClearChunkCells()
    {
        if (_chunkCells is not null)
        {
            Array.Clear(_chunkCells, 0, RoomWidth * RoomHeight);
        }
    }

    public void Dispose()
    {
        var buffer = _chunkCells;
        if (buffer is null)
        {
            return;
        }

        _chunkCells = null;
        // Clear with the pool's helper so we never keep dead Chunk references
        // alive through the pool's retention.
        ArrayPool<Chunk?>.Shared.Return(buffer, clearArray: true);
    }

    // The synthesized record equality only compares the positional + init-only
    // properties. _chunkCells is a manually-managed pool buffer whose identity
    // matters for disposal — two instances constructed with the same args have
    // distinct rented buffers and are NOT interchangeable. Fold the buffer
    // reference into equality so dedup / cache keys don't conflate them.
    public bool Equals(RoomParameters? other)
        => other is not null
            && RoomWidth == other.RoomWidth
            && RoomHeight == other.RoomHeight
            && DefaultTerrain == other.DefaultTerrain
            && DefaultPathTerrain == other.DefaultPathTerrain
            && DefaultWall == other.DefaultWall
            && ReferenceEquals(CreatureGenerator, other.CreatureGenerator)
            && ReferenceEquals(ItemGenerator, other.ItemGenerator)
            && ReferenceEquals(_chunkCells, other._chunkCells);

    public override int GetHashCode()
        => HashCode.Combine(
            RoomWidth, RoomHeight, DefaultTerrain,
            DefaultPathTerrain, DefaultWall,
            CreatureGenerator, ItemGenerator,
            RuntimeHelpers.GetHashCode(_chunkCells!));
}
