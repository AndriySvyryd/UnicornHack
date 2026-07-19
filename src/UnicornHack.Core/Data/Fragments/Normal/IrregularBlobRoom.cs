namespace UnicornHack.Data.Fragments;

public static partial class NormalMapFragmentData
{
    public static readonly NormalMapFragment IrregularBlobRoom = new NormalMapFragment
    {
        Name = "irregularBlobRoom",
        GenerationWeight = "$branch == 'dungeon' || $branch == 'entrance' ? 0 : 1.0 / ((2 + $instances) >> 1)",
        DynamicMap = new IrregularBlobMap { Coverage = 0.7F, MinSize = new Dimensions { Width = 5, Height = 5 } },
        Tags = new HashSet<string> { "door-n", "door-s", "door-e", "door-w" }
    };
}
