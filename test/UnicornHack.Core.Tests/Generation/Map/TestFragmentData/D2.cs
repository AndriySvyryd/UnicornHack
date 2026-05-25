namespace UnicornHack.Generation.Map.TestFragments;

public static partial class TestDefiningMapFragmentData
{
    public static readonly DefiningMapFragment D2 = new DefiningMapFragment
    {
        Name = "d2",
        NoRandomDoorways = true,
        RoomHeight = 5,
        RoomWidth = 5,
        Map = @"
#####
#%%%#
#%<%#
#%%%#
#####"
    };
}
