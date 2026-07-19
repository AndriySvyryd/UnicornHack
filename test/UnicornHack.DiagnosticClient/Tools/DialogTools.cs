using System.ComponentModel;
using ModelContextProtocol.Server;
using UnicornHack.DiagnosticClient.Connection;
using UnicornHack.Hubs;
using UnicornHack.Services;

namespace UnicornHack.DiagnosticClient.Tools;

[McpServerToolType]
public sealed class DialogTools
{
    private readonly GameConnection _conn;
    private readonly DiagnosticOptions _options;

    public DialogTools(GameConnection conn, DiagnosticOptions options)
    {
        _conn = conn;
        _options = options;
    }

    // The following tools each map to a single UnicornHack.Hubs.GameQueryType value. They all dispatch through
    // GameHub.ShowDialog which pushes the result back via ReceiveUIRequest; call get_dialog to inspect the payload.
    // StaticDescription (query type 9) is intentionally not exposed here — use the `describe` tool instead.

    [McpServerTool, Description(
        "Clear the active dialog on the server (GameQueryType.Clear). Server responds via ReceiveUIRequest with an empty payload; " +
        "subsequent get_dialog will return (empty).")]
    public Task<string> clear_dialog(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.Clear, [], cancellationToken);

    [McpServerTool, Description(
        "Request the list of abilities slottable into the given hotbar slot (GameQueryType.SlottableAbilities). " +
        "Call get_dialog to inspect the returned list.")]
    public Task<string> show_slottable_abilities(int slot, CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.SlottableAbilities, [slot], cancellationToken);

    [McpServerTool, Description(
        "Request the player's full attribute sheet (GameQueryType.PlayerAttributes). Call get_dialog to inspect it.")]
    public Task<string> show_player_attributes(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.PlayerAttributes, [], cancellationToken);

    [McpServerTool, Description(
        "Request the player's inventory (GameQueryType.PlayerInventory). Call get_dialog to inspect it.")]
    public Task<string> show_player_inventory(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.PlayerInventory, [], cancellationToken);

    [McpServerTool, Description(
        "Request the player's adaptations/traits panel (GameQueryType.PlayerAdaptations). Call get_dialog to inspect it.")]
    public Task<string> show_player_adaptations(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.PlayerAdaptations, [], cancellationToken);

    [McpServerTool, Description(
        "Request the player's skill panel (GameQueryType.PlayerSkills). Call get_dialog to inspect it.")]
    public Task<string> show_player_skills(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.PlayerSkills, [], cancellationToken);

    [McpServerTool, Description(
        "Request the detailed attributes of the given ability entity id (GameQueryType.AbilityAttributes). " +
        "Use get_abilities to discover ability ids. Call get_dialog to inspect the result.")]
    public Task<string> show_ability_attributes(int ability_id, CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.AbilityAttributes, [ability_id], cancellationToken);

    [McpServerTool, Description(
        "Request the detailed attributes of the given actor entity id (GameQueryType.ActorAttributes). " +
        "Use get_known_actors to discover actor ids. Call get_dialog to inspect the result.")]
    public Task<string> show_actor_attributes(int actor_id, CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.ActorAttributes, [actor_id], cancellationToken);

    [McpServerTool, Description(
        "Request the detailed attributes of the given item entity id (GameQueryType.ItemAttributes). " +
        "Use get_items to discover item ids. Call get_dialog to inspect the result.")]
    public Task<string> show_item_attributes(int item_id, CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.ItemAttributes, [item_id], cancellationToken);

    [McpServerTool, Description(
        "Request the post-game statistics screen (GameQueryType.PostGameStatistics). Server currently does not implement " +
        "this branch and may throw; call get_dialog to inspect the result if any.")]
    public Task<string> show_post_game_statistics(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.PostGameStatistics, [], cancellationToken);

    [McpServerTool, Description(
        "Request a snapshot of the current level map (GameQueryType.LevelMap). Call get_dialog to inspect it.")]
    public Task<string> show_level_map(CancellationToken cancellationToken = default)
        => ShowDialog(GameQueryType.LevelMap, [], cancellationToken);

    private Task<string> ShowDialog(GameQueryType queryType, int[] arguments, CancellationToken cancellationToken)
        => WrapResult(
            $"show_dialog {queryType}",
            "ShowDialog",
            async ct =>
            {
                var payload = await _conn.ShowDialogAsync((int)queryType, arguments, ct).ConfigureAwait(false);
                return DialogPayloadFormatter.Render(payload);
            },
            cancellationToken);

    [McpServerTool, Description(
        "Fetch a localized description from the server. category mirrors DescriptionCategory: " +
        "0=Creature 1=Item 2=Ability 3=Trait 4=Feat 5=Race 6=God 7=Player 8=Spell 9=Other " +
        "(see UnicornHack.Services.DescriptionCategory for the canonical enum).")]
    public Task<string> describe(string topic_id, int category, CancellationToken cancellationToken = default)
        => WrapResult(
            $"describe topic_id={topic_id} category={category}",
            "QueryStaticDescription",
            ct => _conn.QueryStaticDescriptionAsync(topic_id, category, ct),
            cancellationToken);

    [McpServerTool, Description(
        "Send a chat message. NOTE: the server's SendMessage reads Context.User.Identity.Name which is unset " +
        "when authentication is disabled, so this will currently throw on the server.")]
    public Task<string> send_message(string message, CancellationToken cancellationToken = default)
        => Wrap(
            "send_message",
            "SendMessage",
            ct => _conn.SendMessageAsync(message, ct),
            cancellationToken);

    private async Task<string> Wrap(string label, string hubMethod, Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.ActionTimeoutMs);
        try
        {
            await work(cts.Token).ConfigureAwait(false);
            return $"{label}: ok";
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return $"error: {label} timed out after {_options.ActionTimeoutMs}ms";
        }
        catch (Exception ex)
        {
            return $"error: {label} :: {ex.GetType().Name}: {Strip(ex.Message, hubMethod)}";
        }
    }

    private async Task<string> WrapResult(string label, string hubMethod, Func<CancellationToken, Task<string>> work, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.ActionTimeoutMs);
        try
        {
            return await work(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return $"error: {label} timed out after {_options.ActionTimeoutMs}ms";
        }
        catch (Exception ex)
        {
            return $"error: {label} :: {ex.GetType().Name}: {Strip(ex.Message, hubMethod)}";
        }
    }

    private static string Strip(string message, string hubMethod)
    {
        var prefix = $"An unexpected error occurred invoking '{hubMethod}' on the server. ";
        return message.StartsWith(prefix, StringComparison.Ordinal) ? message[prefix.Length..] : message;
    }
}
