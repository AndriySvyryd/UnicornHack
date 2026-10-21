using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

public sealed class Reward : NotificationEntity
{
    public int Id
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public int FeatureId
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public string Title
    {
        get;
        set => SetWithNotify(value, ref field);
    } = "";

    public RewardType Type
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public string[] Names
    {
        get;
        set => SetWithNotify(value, ref field);
    } = [];

    public MapFeature ActiveFeature
    {
        get;
        set => SetWithNotify(value, ref field);
    } = MapFeature.SmallFountain;

    public int? ClaimantEntityId
    {
        get;
        set => SetWithNotify(value, ref field);
    }
}

public enum RewardType
{
    Boon,
    Item,
    Race,
    RacePedestal
}