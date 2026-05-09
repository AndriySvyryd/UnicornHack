namespace UnicornHack.Primitives;

[Flags]
public enum Directionality
{
    None = 0,
    Source = 1 << 0,
    Destination = 1 << 1,
    Both = Source | Destination
}
