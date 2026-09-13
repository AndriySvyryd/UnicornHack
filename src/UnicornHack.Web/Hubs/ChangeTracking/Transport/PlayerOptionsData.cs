using MessagePack;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Hubs.ChangeTracking;

[MessagePackObject]
public class PlayerOptionsData
{
    [Key(0)]
    public string Choice { get; set; } = "";

    [Key(1)]
    public string? Action { get; set; }

    [Key(2)]
    public string Title { get; set; } = "";

    [Key(3)]
    public List<PlayerOptionData> Options { get; set; } = [];

    [Key(4)]
    public string? Notice { get; set; }
}

[MessagePackObject]
public class PlayerOptionData
{
    [Key(0)]
    public string Identifier { get; set; } = "";

    [Key(1)]
    public string Title { get; set; } = "";

    [Key(2)]
    public string Description { get; set; } = "";

    [Key(3)]
    public string? DisabledReason { get; set; }
}
