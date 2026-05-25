namespace UnicornHack.Generation.Map;

/// <summary>
///     <see cref="NormalMapFragment" /> subclass that exposes the protected
///     <see cref="MapFragment.Width" /> and <see cref="MapFragment.Height" /> setters so
///     tests can pre-size a fragment to match an external <see cref="RoomComponent" />
///     before <see cref="MapFragment.EnsureInitialized" /> parses its <c>Map</c>.
/// </summary>
internal sealed class TestNormalMapFragment : NormalMapFragment
{
    public void SetDimensions(byte width, byte height)
    {
        Width = width;
        Height = height;
    }
}

/// <summary>
///     <see cref="DefiningMapFragment" /> subclass that exposes the protected
///     <see cref="MapFragment.ByteMap" /> setter so tests can swap the parsed byte map
///     (e.g. to temporarily zero it out while routing the fragment through
///     <see cref="LevelGenerator" />, then restore it for downstream assertions).
/// </summary>
internal sealed class TestDefiningMapFragment : DefiningMapFragment
{
    public void SetByteMap(byte[]? byteMap)
        => ByteMap = byteMap;
}
