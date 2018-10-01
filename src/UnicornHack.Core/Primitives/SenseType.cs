using System;

namespace UnicornHack.Primitives
{
    [Flags]
    public enum SenseType : byte
    {
        None = 0,
        Sight = 1 << 0,
        Sound = 1 << 1,
        SoundDistant = 1 << 2,
        Danger = 1 << 3,
        Telepathy = 1 << 4,
        TelepathyWeak = 1 << 5,
        Touch = 1 << 6
    }

    public static class SenseTypeExtentions
    {
        public static bool CanIdentify(this SenseType sense)
            => (sense & (SenseType.Sight | SenseType.Telepathy)) != SenseType.None;
    }
}
