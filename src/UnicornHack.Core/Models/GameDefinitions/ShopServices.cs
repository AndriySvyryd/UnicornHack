using System;

namespace UnicornHack.Models.GameDefinitions
{
    [Flags]
    public enum ShopServices
    {
        None = 0,
        Identify = 1 << 0,
        Uncurse = 1 << 1,
        Enhance = 1 << 2,
        Enchant = 1 << 3
    }
}