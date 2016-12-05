using System;

namespace UnicornHack
{
    public enum MapFeature : byte
    {
        Default = 0,
        Corridor = 1,
        Floor = 2,
        WallVertical = 3,
        WallHorizontal = 4,
        WallTopLeft = 5,
        WallTopRight = 6,
        WallBottomLeft = 7,
        WallBottomRight = 8,
        WallCross = 9,
        WallTUp = 10,
        WallTDown = 11,
        WallTLeft = 12,
        WallTRight = 13,
        Pool = 14
    }

    public static class MapFeatureExtensions
    {
        // TODO: use locomotion type mask (walking, flying, swimming, heavy, phasing)
        public static bool CanMoveTo(this MapFeature feature)
        {
            switch (feature)
            {
                case MapFeature.Default:
                case MapFeature.WallVertical:
                case MapFeature.WallHorizontal:
                case MapFeature.WallTopLeft:
                case MapFeature.WallTopRight:
                case MapFeature.WallBottomLeft:
                case MapFeature.WallBottomRight:
                case MapFeature.WallCross:
                case MapFeature.WallTUp:
                case MapFeature.WallTDown:
                case MapFeature.WallTLeft:
                case MapFeature.WallTRight:
                    return false;
                case MapFeature.Floor:
                case MapFeature.Corridor:
                case MapFeature.Pool:
                    return true;
                default:
                    throw new NotSupportedException($"Map feature {feature} not supported.");
            }
        }
    }
}