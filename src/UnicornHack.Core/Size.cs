using System;

namespace UnicornHack
{
    [Flags]
    public enum Size
    {
        None = 0,

        /// <summary> &lt; 2' </summary>
        Tiny = 1 << 0,

        /// <summary> 2-4' </summary>
        Small = 1 << 1,

        /// <summary> 4-7' </summary>
        Medium = 1 << 2,

        /// <summary> 7-12' </summary>
        Large = 1 << 3,

        /// <summary> 12-25' </summary>
        Huge = 1 << 4,

        /// <summary> &gt; 25' </summary>
        Gigantic = 1 << 5
    }
}