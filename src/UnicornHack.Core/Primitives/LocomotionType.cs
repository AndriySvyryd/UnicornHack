using System;

namespace UnicornHack.Primitives;

[Flags]
public enum LocomotionType
{
    None = 0,
    Walking = 1 << 0,
    Flying = 1 << 1,
    Swimming = 1 << 2,
    Teleportation = 1 << 3,
    Phasing = 1 << 4,
    Tunneling = 1 << 5,
    ToolTunneling = 1 << 6,
    WaterWalking = 1 << 7
}
