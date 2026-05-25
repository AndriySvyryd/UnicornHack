namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    /// <summary>
    ///     Sentinel <see cref="NormalMapFragment" /> instance used as the
    ///     <see cref="Chunk.Fragment" /> for the synthesized rectangles produced by
    ///     <see cref="MapFragment.FixupDeferredConditions" />. Its <see cref="MapFragment.SourceName" />
    ///     is <c>$deferred</c> so per-fragment statistics treat the fixup pass as a single
    ///     virtual source. <see cref="MapFragment.GenerationWeight" /> is <c>"0"</c> so it
    ///     is never selected during normal fragment picking, and
    ///     <see cref="MapFragment.NoTransform" /> suppresses D4 orbit expansion.
    /// </summary>
    public static readonly NormalMapFragment DeferredFillFragment = new NormalMapFragment
    {
        Name = "$deferred",
        DynamicMap = new RectangleMap { MinSize = new Dimensions { Width = 1, Height = 1 } },
        GenerationWeight = "0",
        NoTransform = true
    };
}
