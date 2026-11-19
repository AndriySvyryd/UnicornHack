using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Features;

[Component(Id = (int)EntityComponent.Feature)]
public class FeatureComponent : GameComponent
{
    public FeatureComponent()
    {
        ComponentId = (int)EntityComponent.Feature;
    }

    public string Name
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            Template = null;
            SetWithNotify(value, ref field);
        }
    } = "";

    public Feature? Template
    {
        get => field ??= Entity.Game.Services.Data.Features.Find(Name);
        set;
    }

    public bool? Destroyed
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public byte UsedByPlayers
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public bool BlocksMovement => Template?.Behavior.HasFlag(FeatureBehavior.BlocksMovement) ?? true;

    public int? NextActionTick
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public SimpleRandom RewardRandom
    {
        get;
        set => SetWithNotify(value, ref field);
    } = new();

    public Dictionary<Point, byte>? ThreatenedCells { get; set; }

    protected override void Clean()
    {
        ThreatenedCells?.Clear();
        ThreatenedCells = null;
        Name = "";
        Destroyed = null;
        UsedByPlayers = 0;
        NextActionTick = null;
        RewardRandom = new();
        Template = null;
        base.Clean();
    }
}