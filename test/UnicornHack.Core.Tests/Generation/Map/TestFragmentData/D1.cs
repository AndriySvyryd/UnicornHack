namespace UnicornHack.Generation.Map.TestFragments;

/// <summary>
///     Test-only defining fragments. Lives outside <c>UnicornHack.Data.Fragments.DefiningMapFragmentData</c>
///     so the production fragment loader's reflection pass doesn't pick them up.
/// </summary>
public static partial class TestDefiningMapFragmentData
{
    public static readonly DefiningMapFragment D1 = new DefiningMapFragment
    {
        Name = "d1",
        NoRandomDoorways = true,
        Map = @"
                     ###########                 #######
                     #..........,,,,,,,,,,,,,,,,,.......,,
 ########           ##..=......#,,   #####,      #.....# ,
 #......#            #..........,,,, #.%.#,      #.....# ,,,
 #......#            #.........#,  , #>...,      ##.####   ,    ###########
 #......#  ,         ########.##,  ,,....#,,,,,,,,,,,,,,,,,,,,,,..........#
 #......., ,,,,,,,,,,,,,,,,,,,,,,   ,#####,        ,,,       ,  #......%..#
 #...%..#,                   ,      ,,,,  ,          ,       ,,,#.........#
 #.$....#,                   ,         ,  ,          ,,,       ,.......(..#
 ######.#,,,                 ,         ,,,,            ,        #.........#
       ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,#    ###########
           ,,, ##############.#           , ###########.#####
             , #...............,,,,,,,,   , #..............#
             ,,....%..........#           , #$#.....%...<..#
               #........).....#           ,,...............#
               #..............#             ################
               #####.##########
                   #,
                    ,
                    ,
                    ,
                    ,
                    ,
                    ,
                    ,
               #####.#######
               #...........#
               #........$..#
               #...........#
               #########.###
                        ,
                        ,
                        ,
                        ,
                        .#####
                        #....#
                        #....#
                        #..%.#
                        #....#
                        ######
"
    };
}
