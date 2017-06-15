using System;

namespace UnicornHack
{
    public enum MapFeature : byte
    {
        Default = 0,
        RockFloor = 1,
        StoneFloor = 2,
        RockWall = 3,
        StoneWall = 4,
        StoneArchway = 5,
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
                case MapFeature.StoneWall:
                case MapFeature.RockWall:
                    return false;
                case MapFeature.StoneFloor:
                case MapFeature.RockFloor:
                case MapFeature.StoneArchway:
                case MapFeature.Pool:
                    return true;
                default:
                    throw new NotSupportedException($"Map feature {feature} not supported.");
            }
        }
    }
}