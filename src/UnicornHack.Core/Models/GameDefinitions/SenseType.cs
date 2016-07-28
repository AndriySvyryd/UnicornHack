using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum SenseType : byte
    {
        None,
        Sight = 1 << 0,
        Sound = 1 << 1,
        SoundDistant = 1 << 2,
        Danger = 1 << 3,
        Telepathy = 1 << 4,
        Touch = 1 << 5
    }
}