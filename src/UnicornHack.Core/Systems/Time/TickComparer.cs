namespace UnicornHack.Systems.Time;

public sealed class TickComparer : IComparer<(int Tick, int ComponentId, int Id)>
{
    public static readonly TickComparer Instance = new();

    private TickComparer()
    {
    }

    public int Compare((int Tick, int ComponentId, int Id) left, (int Tick, int ComponentId, int Id) right)
    {
        var diff = left.Tick.CompareTo(right.Tick);
        if (diff != 0)
        {
            return diff;
        }

        diff = left.ComponentId.CompareTo(right.ComponentId);
        return diff != 0 ? diff : left.Id.CompareTo(right.Id);
    }
}
