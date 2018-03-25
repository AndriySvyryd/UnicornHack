using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum TargetingType
    {
        None = 0,
        AdjacentSingle = 1 << 0,
        AdjacentArc = 1 << 1,
        Projectile = 1 << 2,
        GuidedProjectile = 1 << 3,
        Beam = 1 << 4,
        LineOfSight = 1 << 5
    }
}
