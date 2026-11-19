using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Systems.Actors;

public sealed class PlayerOptions : NotificationEntity
{
    public int? SourceFeatureId
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public string? SelectedOption
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public RewardType ChoiceType
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public string? PendingChoiceAction
    {
        get;
        set => SetWithNotify(value, ref field);
    }

    public List<string> PendingChoiceOptions
    {
        get;
        set => SetWithNotify(value, ref field);
    } = [];
}

public sealed record PlayerChoicePresentation(
    RewardType Choice,
    string? Action,
    string Title,
    IReadOnlyList<PlayerChoiceOptionPresentation> Options,
    string? Notice = null);

public sealed record PlayerChoiceOptionPresentation(
    string Identifier,
    string Title,
    string Description,
    string? DisabledReason = null);

public readonly record struct PlayerOptionSelectionResult(
    bool IsAccepted,
    int Delay,
    string? ErrorMessage)
{
    public static PlayerOptionSelectionResult Complete(int delay = 0)
        => new(true, delay, null);

    public static PlayerOptionSelectionResult Reject(string errorMessage)
        => new(false, 0, errorMessage);
}
