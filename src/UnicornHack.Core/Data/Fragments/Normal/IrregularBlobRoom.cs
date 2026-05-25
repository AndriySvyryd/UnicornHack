namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    // Dynamic organic non-rectangular floor blob; softer alternative to RandomRectangle
    // for non-dungeon branches.
    public static readonly NormalMapFragment IrregularBlobRoom = new NormalMapFragment
    {
        Name = "irregularBlobRoom",
        GenerationWeight = "$branch == 'dungeon' || $branch == 'entrance' ? 0 : 1.0 / ((2 + $instances) >> 1)",
        DynamicMap = new IrregularBlobMap { MinSize = new Dimensions { Width = 5, Height = 5 } },
        Tags = ["door-n", "door-s", "door-e", "door-w"]
    };
}
