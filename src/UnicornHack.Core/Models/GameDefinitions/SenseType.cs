using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum SenseType : byte
    {
        None,
        Sight = 1 << 0,
        Sound = 1 << 1,
        Danger = 1 << 2,
        Touch = 1 << 3
    }
}