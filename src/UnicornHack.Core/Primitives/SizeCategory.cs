namespace UnicornHack.Primitives;

[Flags]
public enum SizeCategory
{
    None = 0,

    /// <summary> &lt; 2' </summary>
    Tiny = 1 << 0,

    /// <summary> 2-4' </summary>
    Small = 1 << 1,

    /// <summary> 4-8' </summary>
    Medium = 1 << 2,

    /// <summary> 8-13' </summary>
    Large = 1 << 3,

    /// <summary> 13-25' </summary>
    Huge = 1 << 4,

    /// <summary> &gt; 25' </summary>
    Gigantic = 1 << 5,

    All = Tiny | Small | Medium | Large | Huge | Gigantic
}
