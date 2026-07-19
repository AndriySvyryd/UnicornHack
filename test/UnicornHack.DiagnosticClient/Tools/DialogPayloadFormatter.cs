using System.Text.Json;
using UnicornHack.Hubs;

namespace UnicornHack.DiagnosticClient.Tools;

/// <summary>
///     Renders typed dialog payloads returned by <c>GameHub.QueryGame</c>.
/// </summary>
public static class DialogPayloadFormatter
{
    public static string Render(DialogData? payload)
        => payload == null
            ? "(empty)"
            : JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
}
