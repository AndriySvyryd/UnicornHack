using System;

namespace UnicornHack
{
    public enum MapFeature : byte
    {
        Default = 0,
        Corridor = 1,
        Floor = 2,
        Wall = 3,
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
                case MapFeature.Wall:
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