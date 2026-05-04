using MessagePack;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class AttackSummary
{
    [Key(0)]
    public int Delay { get; set; }

    [Key(1)]
    public string HitProbabilities { get; set; } = "";

    [Key(2)]
    public string Damages { get; set; } = "";

    [Key(3)]
    public int TicksToKill  { get; set; }
}
