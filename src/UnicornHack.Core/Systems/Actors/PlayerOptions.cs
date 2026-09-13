using UnicornHack.Utils;

namespace UnicornHack.Systems.Actors;

public sealed class PlayerOptions : NotificationEntity
{
    private string _pendingChoice = "";
    private string? _pendingChoiceAction;
    private string[] _pendingChoiceOptions = [];

    public string ChoiceType
    {
        get => _pendingChoice;
        set => SetWithNotify(value, ref _pendingChoice);
    }

    public string? PendingChoiceAction
    {
        get => _pendingChoiceAction;
        set => SetWithNotify(value, ref _pendingChoiceAction);
    }

    public string[] PendingChoiceOptions
    {
        get => _pendingChoiceOptions;
        set => SetWithNotify(value, ref _pendingChoiceOptions);
    }
}

public sealed record PlayerChoicePresentation(
    string Choice,
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
